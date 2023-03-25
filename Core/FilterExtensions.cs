#if UNITY_EDITOR
#define MORPEH_DEBUG
#define MORPEH_PROFILING
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Collections;
    using JetBrains.Annotations;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class FilterExtensions {
        internal static void Dispose(this Filter filter) {
            foreach (var child in filter.childs) {
                child.Dispose();
            }

            filter.childs.Clear();
            filter.childs = null;

            if (filter.archetypes != null) {
                filter.archetypes.Clear();
                filter.archetypes = null;
                
                filter.chunks.Clear();
                filter.chunks = null;
            }
            
            filter.includedTypeIds?.Clear();
            filter.includedTypeIds = null;
            filter.excludedTypeIds?.Clear();
            filter.excludedTypeIds = null;

            filter.typeID = -1;
            filter.mode   = Filter.Mode.None;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FindArchetypes(this Filter filter, FastList<long> newArchetypes) {
            foreach (var archId in newArchetypes) {
                var arch = filter.world.archetypes.GetValueByKey(archId);
                filter.CheckArchetype(arch);
            }
            if (filter.chunks.capacity < filter.archetypes.length) {
                filter.chunks.Resize(filter.archetypes.capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FindArchetypes(this Filter filter) {
            foreach (var arch in filter.world.archetypes) {
                filter.CheckArchetype(filter.world.archetypes.GetValueByIndex(arch));
            }
            if (filter.chunks.capacity < filter.archetypes.length) {
                filter.chunks.Resize(filter.archetypes.capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckArchetype(this Filter filter, Archetype archetype) {
            var e = archetype.entities.First();
            var ent = filter.world.GetEntity(e);

            foreach (var includedTypeId in filter.includedTypeIds) {
                var stash = filter.world.GetStash(includedTypeId);
                if (stash == null) {
                    return;
                }
                if (!stash.Has(ent)) {
                    return;
                }
            }
            
            foreach (var excludedTypeId in filter.excludedTypeIds) {
                var stash = filter.world.GetStash(excludedTypeId);
                if (stash == null) {
                    continue;
                }
                if (stash.Has(ent)) {
                    return;
                }
            }

            filter.archetypes.Add(archetype);
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetEntity(this Filter filter, in int id) {
            // ReSharper disable once GenericEnumeratorNotDisposed
            var enumerator = filter.GetEnumerator();
            for (int i = 0, length = id + 1; i < length; i++) {
                if (enumerator.MoveNext() == false) {
                    throw new IndexOutOfRangeException();
                }
            }

            return enumerator.Current;
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity First(this Filter filter) {
            // ReSharper disable once GenericEnumeratorNotDisposed
            var enumerator = filter.GetEnumerator();
            if (enumerator.MoveNext()) {
                return enumerator.Current;
            }

            throw new InvalidOperationException("The source sequence is empty.");
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity FirstOrDefault(this Filter filter) {
            // ReSharper disable once GenericEnumeratorNotDisposed
            var enumerator = filter.GetEnumerator();
            if (enumerator.MoveNext()) {
                return enumerator.Current;
            }

            return default;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLengthSlow(this Filter filter) {
            filter.world.ThreadSafetyCheck();
            int accum = 0;
            foreach (var arch in filter.archetypes) {
                if (arch.usedInNative) {
                    accum += arch.entitiesNative.length;
                }
                else {
                    accum += arch.entities.count;
                }
            }
            return accum;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this Filter filter) {
            filter.world.ThreadSafetyCheck();
            
            foreach (var arch in filter.archetypes) {
                if (arch.usedInNative) {
                    if (arch.entitiesNative.length > 0) {
                        return false;
                    }
                }
                else {
                    if (arch.entities.count > 0) {
                        return false;
                    }
                }
            }
            return true;
        }

        public static FilterBuilder With<T>(this FilterBuilder builder) where T : struct, IComponent 
            => new FilterBuilder {
                parent = builder,
                world = builder.world,
                typeId = TypeIdentifier<T>.info.id
            };

        public static FilterBuilder Without<T>(this FilterBuilder builder) where T : struct, IComponent
            => new FilterBuilder {
                parent = builder,
                world = builder.world,
                typeId = -TypeIdentifier<T>.info.id
            };

        public static FilterBuilder Extend<T>(this FilterBuilder builder) where T : struct, IFilterExtension {
// #if MORPEH_DEBUG 
//             var check = filter.gen;
// #endif
            var newFilter = default(T).Extend(builder);
// #if MORPEH_DEBUG 
//             if (check == filter.gen) {
//                 MLogger.LogError("[MORPEH] You didn't extend the filter in any way, perhaps you mistyped the IFilterExtension?");
//             }
// #endif
            return newFilter;
        }

        public static Filter Build(this FilterBuilder builder) {
            //todo: legacy fallback
            var current = builder;
            var stack = new Stack<FilterBuilder>();
            while (current.parent != null) {
                stack.Push(current);
                current = current.parent;
            }
            var filter = builder.world.LegacyRootFilter;
            while (stack.Count > 0) {
                current = stack.Pop();
                filter = filter.CreateFilter(System.Math.Abs(current.typeId), current.typeId < 0 ? Filter.Mode.Exclude : Filter.Mode.Include);
            }
            return filter;
        }
        
        private static Filter CreateFilter(this Filter filter, long currentTypeId, Filter.Mode mode) {
            filter.gen++;
            if (filter.includedTypeIds != null) {
                foreach (var typeId in filter.includedTypeIds) {
                    if (typeId == currentTypeId) {
                        return filter;
                    }
                }
            }
            if (filter.excludedTypeIds != null) {
                foreach (var typeId in filter.excludedTypeIds) {
                    if (typeId == currentTypeId) {
                        return filter;
                    }
                }
            }

            for (int i = 0, length = filter.childs.length; i < length; i++) {
                var child = filter.childs.data[i];
                if (child.mode == mode && child.typeID == currentTypeId) {
                    return child;
                }
            }


            FastList<long> newIncludedTypeIds;
            FastList<long> newExcludedTypeIds;
            if (filter.typeID == -1) {
                newIncludedTypeIds = new FastList<long>();
                newExcludedTypeIds = new FastList<long>();
            }
            else {
                newIncludedTypeIds = new FastList<long>(filter.includedTypeIds);
                newExcludedTypeIds = new FastList<long>(filter.excludedTypeIds);
            }

            if (mode == Filter.Mode.Include) {
                newIncludedTypeIds.Add(currentTypeId);
            }
            else if (mode == Filter.Mode.Exclude) {
                newExcludedTypeIds.Add(currentTypeId);
            }

            var newFilter = new Filter(filter.world, currentTypeId, newIncludedTypeIds, newExcludedTypeIds, mode);
            filter.childs.Add(newFilter);

            return newFilter;
        }
    }
}