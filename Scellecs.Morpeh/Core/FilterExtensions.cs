#if UNITY_EDITOR
#define MORPEH_DEBUG
#define MORPEH_PROFILING
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
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
                
                filter.archetypes = null;
                filter.archetypesLength = 0;
                filter.archetypesCapacity = 0;

                filter.chunks.Clear();
                filter.chunks = null;
            }
            
            filter.includedOffsets = null;
            filter.excludedOffsets = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool AddArchetypeIfMatches(this Filter filter, Archetype archetype) {
            if (filter.includedOffsets.Length > archetype.components.length) {
                return false;
            }
            
            if (filter.archetypeHashes.Has(archetype.hash.GetValue())) {
                MLogger.LogTrace($"Archetype {archetype.hash} already in filter {filter}");
                return false;
            }
            
            if (!filter.ArchetypeMatches(archetype)) {
                return false;
            }

            var index = filter.archetypesLength++;
            if (index >= filter.archetypesCapacity) {
                filter.ResizeArchetypes(filter.archetypesCapacity << 1);
            }
            
            filter.archetypes[index] = archetype;
            filter.archetypeHashes.Add(archetype.hash.GetValue(), index, out _);
            
            if (filter.chunks.capacity < filter.archetypesLength) {
                filter.chunks.Resize(filter.archetypesCapacity);
            }
            
            return true;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ResizeArchetypes(this Filter filter, int newCapacity) {
            ArrayHelpers.Grow(ref filter.archetypes, newCapacity);
            filter.archetypesCapacity = newCapacity;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveArchetype(this Filter filter, Archetype archetype) {
            if (!filter.archetypeHashes.Remove(archetype.hash.GetValue(), out var index)) {
                MLogger.LogTrace($"Archetype {archetype.hash} is not in filter {filter}");
                return;
            }
    
            var lastIndex = --filter.archetypesLength;
            filter.archetypes[index] = filter.archetypes[lastIndex];
            
            if (index < lastIndex) {
                filter.archetypeHashes.Set(filter.archetypes[index].hash.GetValue(), index, out _);
            }
            
            filter.archetypes[lastIndex] = default;
        }

        internal static bool ArchetypeMatches(this Filter filter, Archetype archetype) {
            var archetypeComponents = archetype.components;
            
            var includedTypes = filter.includedOffsets;
            var includedTypesLength = includedTypes.Length;
            
            for (var i = 0; i < includedTypesLength; i++) {
                if (!archetypeComponents.Has(includedTypes[i].GetValue())) {
                    MLogger.LogTrace($"Archetype {archetype.hash} does not match filter {filter} [include]");
                    return false;
                }
            }
            
            var excludedTypes = filter.excludedOffsets;
            var excludedTypesLength = excludedTypes.Length;
            
            for (var i = 0; i < excludedTypesLength; i++) {
                if (archetypeComponents.Has(excludedTypes[i].GetValue())) {
                    MLogger.LogTrace($"Archetype {archetype.hash} does not match filter {filter} [exclude]");
                    return false;
                }
            }
            
            MLogger.LogTrace($"Archetype {archetype.hash} matches filter {filter}");
            return true;
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity GetEntity(this Filter filter, in int id) {
            var accum = 0;
            
            var archetypesLength = filter.archetypesLength;
            
            for (var i = 0; i < archetypesLength; i++) {
                var archetype = filter.archetypes[i];
                
                var length = archetype.length;
                if (id < accum + length) {
                    return archetype.entities.data[id - accum];
                }
                accum += length;
            }
            
            ThrowIndexOutOfRange();
            return default;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowIndexOutOfRange() {
            throw new IndexOutOfRangeException();
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity First(this Filter filter) {
            if (filter.archetypesLength == 0) {
                ThrowSourceIsEmpty();
            }
            
            return filter.archetypes[0].entities.data[0];
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowSourceIsEmpty() {
            throw new InvalidOperationException("The source sequence is empty.");
        }

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity FirstOrDefault(this Filter filter) {
            return filter.archetypesLength != 0 ? filter.archetypes[0].entities.data[0] : default;
        }
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLengthSlow(this Filter filter) {
            filter.world.ThreadSafetyCheck();
            var accum = 0;

            var archetypesLength = filter.archetypesLength;
            
            for (var i = 0; i < archetypesLength; i++) {
                accum += filter.archetypes[i].length;
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
            var includedOffsets = new FastList<TypeOffset>(builder.level);
            var excludedOffsets = new FastList<TypeOffset>(builder.level);
            
            var current = builder;

            while (current.parent != null) {
                if (current.mode == Filter.Mode.Include) {
                    includedOffsets.Add(current.typeInfo.offset);
                }
                else if (current.mode == Filter.Mode.Exclude) {
                    excludedOffsets.Add(current.typeInfo.offset);
                }
                
                current = current.parent;
            }
            
            var includedOffsetsSortedArray = includedOffsets.ToArray();
            Array.Sort(includedOffsetsSortedArray, (a, b) => a.GetValue().CompareTo(b.GetValue()));
            
            var excludedOffsetsSortedArray = excludedOffsets.ToArray();
            Array.Sort(excludedOffsetsSortedArray, (a, b) => a.GetValue().CompareTo(b.GetValue()));

            var filter = new Filter(builder.world, includedOffsetsSortedArray, excludedOffsetsSortedArray);

            filter.world.componentsToFiltersRelation.Add(includedOffsetsSortedArray, filter);
            filter.world.componentsToFiltersRelation.Add(excludedOffsetsSortedArray, filter);
            
            filter.world.filters.Add(filter);
            
            foreach (var archetypeIndex in filter.world.archetypes) {
                var archetype = filter.world.archetypes.GetValueByIndex(archetypeIndex);
                if (filter.AddArchetypeIfMatches(archetype)) {
                    archetype.AddFilter(filter);
                }
            }
            
            return filter;
        }
    }
}
