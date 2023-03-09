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
    using System.Runtime.CompilerServices;
    using Collections;
    using JetBrains.Annotations;
    using Unity.IL2CPP.CompilerServices;

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
                foreach (var archetype in filter.archetypes) {
                    archetype.RemoveFilter(filter);
                }

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
        internal static void FindArchetypes(this Filter filter, IntFastList newArchetypes) {
            var minLength = filter.includedTypeIds.length;
            foreach (var archId in newArchetypes) {
                var arch = filter.world.archetypes.data[archId];
                filter.CheckArchetype(arch, minLength);
            }
            if (filter.chunks.capacity < filter.archetypes.length) {
                filter.chunks.Resize(filter.archetypes.capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FindArchetypes(this Filter filter) {
            var minLength = filter.includedTypeIds.length;
            foreach (var arch in filter.world.archetypes) {
                filter.CheckArchetype(arch, minLength);
            }
            if (filter.chunks.capacity < filter.archetypes.length) {
                filter.chunks.Resize(filter.archetypes.capacity);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckArchetype(this Filter filter, Archetype archetype, int minLength) {
            var typeIdsLength = archetype.typeIds.Length;
            if (typeIdsLength >= minLength) {
                var check = true;
                for (int i = 0, length = minLength; i < length; i++) {
                    var includedTypeId = filter.includedTypeIds.Get(i);
                    var foundInclude   = false;
                    for (int j = 0, lengthj = typeIdsLength; j < lengthj; j++) {
                        var typeId = archetype.typeIds[j];
                        if (typeId > includedTypeId) {
                            check = false;
                            goto BREAK;
                        }

                        if (typeId == includedTypeId) {
                            foundInclude = true;
                            break;
                        }
                    }

                    if (foundInclude == false) {
                        check = false;
                        goto BREAK;
                    }
                }

                for (int i = 0, length = filter.excludedTypeIds.length; i < length; i++) {
                    var excludedTypeId = filter.excludedTypeIds.Get(i);
                    for (int j = 0, lengthj = typeIdsLength; j < lengthj; j++) {
                        var typeId = archetype.typeIds[j];
                        if (typeId > excludedTypeId) {
                            break;
                        }

                        if (typeId == excludedTypeId) {
                            check = false;
                            goto BREAK;
                        }
                    }
                }

                BREAK:
                if (check) {
                    for (int i = 0, length = filter.archetypes.length; i < length; i++) {
                        if (filter.archetypes.data[i] == archetype) {
                            return;
                        }
                    }

                    filter.archetypes.Add(archetype);
                    archetype.AddFilter(filter);
                }
            }
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
        
        private static Filter CreateFilter(this Filter filter, int currentTypeId, Filter.Mode mode) {
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


            IntFastList newIncludedTypeIds;
            IntFastList newExcludedTypeIds;
            if (filter.typeID == -1) {
                newIncludedTypeIds = new IntFastList();
                newExcludedTypeIds = new IntFastList();
            }
            else {
                newIncludedTypeIds = new IntFastList(filter.includedTypeIds);
                newExcludedTypeIds = new IntFastList(filter.excludedTypeIds);
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