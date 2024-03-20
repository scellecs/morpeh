#if UNITY_EDITOR
#define MORPEH_DEBUG
#define MORPEH_PROFILING
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Collections;
    using JetBrains.Annotations;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class FilterExtensions {
        internal static void Dispose(this Filter filter) {
            if (filter.archetypes != null) {
                foreach (var archetype in filter.archetypes) {
                    archetype.RemoveFilter(filter);
                }

                filter.archetypes.Clear();
                filter.archetypes = null;

                filter.chunks.Clear();
                filter.chunks = null;
                filter.archetypesLength = 0;
            }
            
            filter.includedTypes?.Clear();
            filter.includedTypes = null;
            filter.excludedTypes?.Clear();
            filter.excludedTypes = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool AddArchetypeIfMatches(this Filter filter, Archetype archetype) {
            if (filter.archetypeIds.Has(archetype.id.GetValue())) {
                MLogger.LogTrace($"Archetype {archetype.id} already in filter {filter}");
                return false;
            }
            
            if (!filter.ArchetypeMatches(archetype)) {
                return false;
            }
        
            filter.archetypeIds.Add(archetype.id.GetValue());
            filter.archetypes.Add(archetype);
            filter.archetypesLength++;
            
            if (filter.chunks.capacity < filter.archetypesLength) {
                filter.chunks.Resize(filter.archetypes.capacity);
            }
            
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void CheckAndAddArchetypes(this Filter filter) {
            foreach (var archetypeIndex in filter.world.archetypes) {
                var archetype = filter.world.archetypes.GetValueByIndex(archetypeIndex);
                if (filter.AddArchetypeIfMatches(archetype)) {
                    archetype.AddFilter(filter);
                }
            }
            if (filter.chunks.capacity < filter.archetypes.length) {
                filter.chunks.Resize(filter.archetypes.capacity);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveArchetype(this Filter filter, Archetype archetype) {
            if (!filter.archetypeIds.Remove(archetype.id.GetValue())) {
                MLogger.LogTrace($"Archetype {archetype.id} is not in filter {filter}");
                return;
            }
            
            filter.archetypes.RemoveSwapSave(archetype, out _);
            filter.archetypesLength--;
        }

        internal static bool ArchetypeMatches(this Filter filter, Archetype archetype) {
            foreach (var includedTypeInfo in filter.includedTypes) {
                if (!archetype.components.Get(includedTypeInfo.offset.GetValue())) {
                    MLogger.LogTrace($"Archetype {archetype.id} does not match filter {filter} [include]");
                    return false;
                }
            }
            
            foreach (var excludedTypeInfo in filter.excludedTypes) {
                if (archetype.components.Get(excludedTypeInfo.offset.GetValue())) {
                    MLogger.LogTrace($"Archetype {archetype.id} does not match filter {filter} [exclude]");
                    return false;
                }
            }
            
            MLogger.LogTrace($"Archetype {archetype.id} matches filter {filter}");
            return true;
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetEntity(this Filter filter, in int id) {
            // ReSharper disable once GenericEnumeratorNotDisposed
            var enumerator = filter.GetEnumerator();
            for (int i = 0, length = id + 1; i < length; i++) {
                if (enumerator.MoveNext() == false) {
#if MORPEH_DEBUG
                    enumerator.Dispose();
#endif
                    throw new IndexOutOfRangeException();
                }
            }
#if MORPEH_DEBUG
            enumerator.Dispose();
#endif
            return enumerator.Current;
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity First(this Filter filter) {
            // ReSharper disable once GenericEnumeratorNotDisposed
            var enumerator = filter.GetEnumerator();
            if (enumerator.MoveNext()) {
#if MORPEH_DEBUG
                enumerator.Dispose();
#endif
                return enumerator.Current;
            }
#if MORPEH_DEBUG
            enumerator.Dispose();
#endif

            throw new InvalidOperationException("The source sequence is empty.");
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity FirstOrDefault(this Filter filter) {
            if (filter.archetypesLength == 0) {
                return default;
            }
            // ReSharper disable once GenericEnumeratorNotDisposed
            var enumerator = filter.GetEnumerator();
            if (enumerator.MoveNext()) {
#if MORPEH_DEBUG
                enumerator.Dispose();
#endif
                return enumerator.Current;
            }
#if MORPEH_DEBUG
            enumerator.Dispose();
#endif

            return default;
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLengthSlow(this Filter filter) {
            filter.world.ThreadSafetyCheck();
            int accum = 0;

            for (int i = 0, length = filter.archetypes.length; i < length; i++) {
                var archetype = filter.archetypes.data[i];
                accum += archetype.entities.count;
            }
            return accum;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this Filter filter) {
            filter.world.ThreadSafetyCheck();

            return filter.archetypesLength == 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotEmpty(this Filter filter) {
            filter.world.ThreadSafetyCheck();

            return filter.archetypesLength != 0;
        }

        public static FilterBuilder With<T>(this FilterBuilder builder) where T : struct, IComponent {
            var typeInfo = TypeIdentifier<T>.info;
            var current = builder;

            while (current.parent != null) {
                if (current.mode == Filter.Mode.Include && current.typeInfo.offset == typeInfo.offset) {
                    return builder;
                }
                current = current.parent;
            }
            
            return new FilterBuilder {
                parent = builder,
                world = builder.world,
                mode = Filter.Mode.Include,
                typeInfo = typeInfo,
                level = builder.level + 1,
                includeHash = builder.includeHash.Combine(typeInfo.id),
                excludeHash = builder.excludeHash,
            };
        }

        public static FilterBuilder Without<T>(this FilterBuilder builder) where T : struct, IComponent {
            var typeInfo = TypeIdentifier<T>.info;
            var current = builder;

            while (current.parent != null) {
                if (current.mode == Filter.Mode.Exclude && current.typeInfo.offset == typeInfo.offset) {
                    return builder;
                }
                current = current.parent;
            }
            
            return new FilterBuilder {
                parent = builder,
                world = builder.world,
                mode = Filter.Mode.Exclude,
                typeInfo = typeInfo,
                level = builder.level + 1,
                includeHash = builder.includeHash,
                excludeHash = builder.excludeHash.Combine(typeInfo.id),
            };
        }

        public static FilterBuilder Extend<T>(this FilterBuilder builder) where T : struct, IFilterExtension {
            var newFilter = default(T).Extend(builder);
            return newFilter;
        }

        public static Filter Build(this FilterBuilder builder) {
            var lookup = builder.world.filtersLookup;

            if (lookup.TryGetValue(builder.includeHash.GetValue(), out var excludeMap)) {
                if (excludeMap.TryGetValue(builder.excludeHash.GetValue(), out var existFilter)) {
                    return existFilter;
                }
                var filter = CompleteBuild(builder);
                excludeMap.Add(builder.excludeHash.GetValue(), filter, out _);
                return filter;
            }
            else {
                var filter = CompleteBuild(builder);
                var newMap = new LongHashMap<Filter>();
                newMap.Add(builder.excludeHash.GetValue(), filter, out _);
                lookup.Add(builder.includeHash.GetValue(), newMap, out _);
                return filter;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Filter CompleteBuild(this FilterBuilder builder) {
            var includedOffsets = new FastList<TypeInfo>();
            var excludedOffsets = new FastList<TypeInfo>();

            var typeOffsets = new TypeInfo[builder.level];
            
            var current = builder;

            while (current.parent != null) {
                if (current.mode == Filter.Mode.Include) {
                    includedOffsets.Add(current.typeInfo);
                }
                else if (current.mode == Filter.Mode.Exclude) {
                    excludedOffsets.Add(current.typeInfo);
                }
                
                if (current.mode != Filter.Mode.None) {
                    typeOffsets[current.level - 1] = current.typeInfo;
                }
                
                current = current.parent;
            }

            var filter = new Filter(builder.world, includedOffsets, excludedOffsets);

            builder.world.componentsToFiltersRelation.Add(typeOffsets, filter);
            filter.world.filters.Add(filter);
            filter.CheckAndAddArchetypes();
            
            return filter;
        }
    }
}
