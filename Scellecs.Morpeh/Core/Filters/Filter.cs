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

        internal SwappableLongSlotMap archetypeHashesMap;
        internal Archetype[] archetypes;
        internal int         archetypesLength;

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

            this.archetypeHashesMap = new SwappableLongSlotMap(FilterConstants.DEFAULT_ARCHETYPES_CAPACITY);
            this.archetypes         = new Archetype[this.archetypeHashesMap.capacity];
            this.archetypesLength    = 0;
            
            this.includedTypeIds = includedTypeIds;
            this.includedTypeIdsLookup = new BitSet(includedTypeIds);
            
            this.excludedTypeIds = excludedTypeIds;
            this.excludedTypeIdsLookup = new BitSet(excludedTypeIds);

            this.id = this.world.freeFilterIDs.TryPop(out var freeID) ? freeID : this.world.filterCount;
            this.world.filterCount++;
            
            this.world.componentsFiltersWith.Add(this.includedTypeIds, this);
            this.world.componentsFiltersWithout.Add(this.excludedTypeIds, this);
            
            foreach (var archetype in this.world.archetypes) {
                if (this.AddArchetypeIfMatches(archetype)) {
                    archetype.AddFilter(this);
                }
            }
        }

        [PublicAPI]
        public void Dispose() {
            this.world.ThreadSafetyCheck();
            if (this.id >= 0) {
                if (this.archetypes != null) {
                    for (int i = 0; i < this.archetypesLength; i++) {
                        var archetype = this.archetypes[i];
                        archetype.RemoveFilter(this);
                    }
#if MORPEH_BURST
#if MORPEH_DEBUG
                    if (this.chunks != null && this.chunks.length > 0) {
                        MLogger.LogWarning("The filter you're trying to dispose of was used in the Native API. Ensure you dispose of it only after all jobs that used it have completed.");
                    }
#endif
                    this.chunks?.Clear();
                    this.chunks = null;
#endif
                    Array.Clear(this.archetypes, 0, this.archetypesLength);
                    this.archetypeHashesMap = null;
                    this.archetypes         = null;
                    this.archetypesLength   = 0;
                }

                var incHash = default(TypeHash);
                var excHash = default(TypeHash);

                if (this.includedTypeIds != null) {
                    for (int i = 0; i < this.includedTypeIds.Length; i++) {
                        var typeId = this.includedTypeIds[i];
                        if (ComponentId.TryGet(typeId, out var type)) {
                            ComponentId.TryGet(type, out var info);
                            incHash = incHash.Combine(info.hash);
                        }
                    }
                    this.world.componentsFiltersWith.Remove(this.includedTypeIds, this);
                    Array.Clear(this.includedTypeIds, 0, this.includedTypeIds.Length);
                    this.includedTypeIds = null;
                }

                if (this.excludedTypeIds != null) {
                    for (int i = 0; i < this.excludedTypeIds.Length; i++) {
                        var typeId = this.excludedTypeIds[i];
                        if (ComponentId.TryGet(typeId, out var type)) {
                            ComponentId.TryGet(type, out var info);
                            excHash = excHash.Combine(info.hash);
                        }
                    }
                    this.world.componentsFiltersWithout.Remove(this.excludedTypeIds, this);
                    Array.Clear(this.excludedTypeIds, 0, this.excludedTypeIds.Length);
                    this.excludedTypeIds = null;
                }

                var lookup = this.world.filtersLookup;
                if (lookup.TryGetValue(incHash.GetValue(), out var excludeMap)) {
                    if (excludeMap.TryGetValue(excHash.GetValue(), out _)) {
                        excludeMap.Remove(excHash.GetValue(), out _);
                    }
                }

                this.includedTypeIdsLookup?.Clear();
                this.includedTypeIdsLookup = null;
                this.excludedTypeIdsLookup?.Clear();
                this.excludedTypeIdsLookup = null;

                this.world.freeFilterIDs.Push(this.id);
                this.world.filterCount--;
                this.id = -1;
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
            return archetype != null && this.archetypeHashesMap.Has(archetype.hash.GetValue());
        }
        
        internal bool AddArchetypeIfMatches(Archetype archetype) {
            if (this.archetypeHashesMap.Has(archetype.hash.GetValue())) {
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
            var slotIndex = this.archetypeHashesMap.TakeSlot(archetype.hash.GetValue(), out var resized);
            
            if (resized) {
                this.ResizeArchetypeHashes(this.archetypeHashesMap.capacity);
            }
            
            this.archetypes[slotIndex] = archetype;
            this.archetypesLength++;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void ResizeArchetypeHashes(int newCapacity) {
            ArrayHelpers.Grow(ref this.archetypes, newCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveArchetype(Archetype archetype) {
            if (!this.archetypeHashesMap.Remove(archetype.hash.GetValue(), out var slotIndex, out var swappedFromSlotIndex)) {
                MLogger.LogTrace($"Archetype {archetype.hash} is not in filter {this}");
                return;
            }
            
            if (swappedFromSlotIndex == -1) {
                this.archetypes[slotIndex] = default;
            } else {
                this.archetypes[slotIndex]            = this.archetypes[swappedFromSlotIndex];
                this.archetypes[swappedFromSlotIndex] = default;
            }
            
            this.archetypesLength--;
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
            Enumerator e;
            
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
