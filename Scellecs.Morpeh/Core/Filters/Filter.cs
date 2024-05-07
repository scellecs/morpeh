#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Collections;
    using JetBrains.Annotations;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class Filter {
#if MORPEH_BURST
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public unsafe struct Chunk {
            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public Entity* entities;
            [ReadOnly]
            public int entitiesLength;
        }
        
        internal FastList<Chunk> chunks;
#endif
        
        internal enum Mode {
            None    = 0,
            Include = 1,
            Exclude = 2
        }

        internal World world;

        internal Archetype[] archetypes;
        internal int archetypesLength;
        internal int archetypesCapacity;
        
        internal LongHashMap<int> archetypeHashes;

        internal int[] includedTypeIds;
        internal BitSet includedTypeIdsLookup;
        
        internal int[] excludedTypeIds;
        internal BitSet excludedTypeIdsLookup;
        
        internal int id;

        [PublicAPI]
        public int ArchetypesCount {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.archetypesLength;
        }

        internal Filter(World world, int[] includedTypeIds, int[] excludedTypeIds) {
            this.world = world;

            this.archetypes = new Archetype[FilterConstants.DEFAULT_ARCHETYPES_CAPACITY];
            this.archetypesLength = 0;
            this.archetypesCapacity = this.archetypes.Length;
            
            this.archetypeHashes = new LongHashMap<int>();
            
            this.includedTypeIds = includedTypeIds;
            this.includedTypeIdsLookup = new BitSet(includedTypeIds);
            
            this.excludedTypeIds = excludedTypeIds;
            this.excludedTypeIdsLookup = new BitSet(excludedTypeIds);

            this.id = world.filterCount++;
            
            this.world.componentsFiltersWith.Add(this.includedTypeIds, this);
            this.world.componentsFiltersWithout.Add(this.excludedTypeIds, this);
            
            foreach (var archetypeIndex in this.world.archetypes) {
                var archetype = this.world.archetypes.GetValueByIndex(archetypeIndex);
                if (this.AddArchetypeIfMatches(archetype)) {
                    archetype.AddFilter(this);
                }
            }
        }
        
        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetLengthSlow() {
            this.world.ThreadSafetyCheck();
            var accum = 0;
            
            for (var i = this.archetypesLength - 1; i >= 0; i--) {
                accum += this.archetypes[i].length;
            }
            
            return accum;
        }
        
        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() {
            this.world.ThreadSafetyCheck();
            return this.archetypesLength == 0;
        }
        
        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotEmpty() {
            this.world.ThreadSafetyCheck();
            return this.archetypesLength != 0;
        }
        
        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity First() {
            this.world.ThreadSafetyCheck();
            
            if (this.archetypesLength == 0) {
                FilterSourceSequenceIsEmptyException.Throw();
            }
            
            return this.archetypes[0].entities.data[0];
        }
        
        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity GetEntity(int position) {
            this.world.ThreadSafetyCheck();
            
            var accum = 0;
            
            for (int i = 0, length = this.archetypesLength; i < length; i++) {
                var archetype = this.archetypes[i];
                
                if (position < accum + archetype.length) {
                    return archetype.entities.data[position - accum];
                }
                
                accum += archetype.length;
            }
            
            FilterIndexOutOfRangeException.Throw(position);
            return default;
        }
        
        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity FirstOrDefault() {
            this.world.ThreadSafetyCheck();
            return this.archetypesLength != 0 ? this.archetypes[0].entities.data[0] : default;
        }
        
        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(Entity entity) {
            if (!this.world.Has(entity)) {
                return false;
            }
            
            var archetype = this.world.entities[entity.Id].currentArchetype;
            return archetype != null && this.archetypeHashes.Has(archetype.hash.GetValue());
        }
        
        internal bool AddArchetypeIfMatches(Archetype archetype) {
            if (this.archetypeHashes.Has(archetype.hash.GetValue())) {
                MLogger.LogTrace($"Archetype {archetype.hash} already in filter {this}");
                return false;
            }
            
            if (!this.ArchetypeMatches(archetype)) {
                return false;
            }

            this.AddArchetype(archetype);
            
            return true;
        }
        
        internal void AddArchetype(Archetype archetype) {
            var index = this.archetypesLength++;
            if (index >= this.archetypesCapacity) {
                this.ResizeArchetypes(this.archetypesCapacity << 1);
            }
            
            this.archetypes[index] = archetype;
            this.archetypeHashes.Add(archetype.hash.GetValue(), index, out _);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void ResizeArchetypes(int newCapacity) {
            ArrayHelpers.Grow(ref this.archetypes, newCapacity);
            this.archetypesCapacity = newCapacity;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveArchetype(Archetype archetype) {
            if (!this.archetypeHashes.Remove(archetype.hash.GetValue(), out var index)) {
                MLogger.LogTrace($"Archetype {archetype.hash} is not in filter {this}");
                return;
            }
    
            var lastIndex = --this.archetypesLength;
            this.archetypes[index] = this.archetypes[lastIndex];
            
            if (index < lastIndex) {
                this.archetypeHashes.Set(this.archetypes[index].hash.GetValue(), index, out _);
            }
            
            this.archetypes[lastIndex] = default;
        }

        internal bool ArchetypeMatches(Archetype archetype) {
            var archetypeComponents = archetype.components;
            
            var includedTypes = this.includedTypeIds;
            for (var i = includedTypes.Length - 1; i >= 0; i--) {
                if (!archetypeComponents.Has(includedTypes[i])) {
                    MLogger.LogTrace($"Archetype {archetype.hash} does not match filter {this} [include]");
                    return false;
                }
            }
            
            var excludedTypes = this.excludedTypeIds;
            for (var i = excludedTypes.Length - 1; i >= 0; i--) {
                if (archetypeComponents.Has(excludedTypes[i])) {
                    MLogger.LogTrace($"Archetype {archetype.hash} does not match filter {this} [exclude]");
                    return false;
                }
            }
            
            MLogger.LogTrace($"Archetype {archetype.hash} matches filter {this}");
            return true;
        }

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            this.world.ThreadSafetyCheck();
            var e = default(Enumerator);
            
            e.entityIndex    = 0;
            e.archetypeIndex = this.archetypesLength;
            
            e.entities       = null;
            e.archetypes     = this.archetypes;

#if MORPEH_DEBUG
            ++this.world.iteratorLevel;
            e.world = this.world;
#endif
            return e;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator
#if MORPEH_DEBUG
            : IDisposable
#endif
        {
            internal int      entityIndex;
            internal int      archetypeIndex;
            internal Entity[] entities;
            internal Archetype[] archetypes;
            
#if MORPEH_DEBUG
            internal World world;
            
            public void Dispose() {
                if (this.world != null) {
                    --this.world.iteratorLevel;
                }
            }
#endif
            
            public Entity Current {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.entities[this.entityIndex];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() {
                if (--this.entityIndex >= 0) {
                    return true;
                }

                if (--this.archetypeIndex < 0) {
                    return false;
                }

                var archetype = this.archetypes[this.archetypeIndex];
                
                this.entities    = archetype.entities.data;
                this.entityIndex = archetype.length - 1;
                
                return true;
            }
        }
    }
}
