namespace Scellecs.Morpeh {
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Collections;
    using JetBrains.Annotations;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FilterBuilder {
        internal World world;
        internal FilterBuilder parent;
        internal long typeId;
        internal Filter.Mode mode;
        internal int level;
    }
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class Filter {
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public unsafe struct Chunk {
            [NativeDisableUnsafePtrRestriction]
            public int* entities;
            public int entitiesLength;
        }
        
        internal enum Mode {
            None    = 0,
            Include = 1,
            Exclude = 2
        }

        internal World world;

        internal LongHashMap<Archetype> archetypes;
        internal FastList<Chunk> chunks;

        internal FastList<long> includedTypeIds;
        internal FastList<long> excludedTypeIds;
        
        
        internal Filter(World world) {
            this.world = world;

            this.archetypes = null;
        }

        internal Filter(World world, FastList<long> includedTypeIds, FastList<long> excludedTypeIds) {
            this.world = world;

            this.archetypes = new LongHashMap<Archetype>();
            this.chunks     = new FastList<Chunk>();

            this.includedTypeIds = includedTypeIds;
            this.excludedTypeIds = excludedTypeIds;

            this.world.filters.Add(this);

            this.AddArchetypes();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityEnumerator GetEnumerator() {
            this.world.ThreadSafetyCheck();
            return new EntityEnumerator(this);
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct EntityEnumerator {
            private readonly LongHashMap<Archetype> archetypes;
            private readonly int                 archetypeCount;

            private int archetypeId;

            private Entity current;

            private World world;

            private BitMap archetypeEntities;
            private UnsafeFastList<int> archetypeEntitiesNative;

            private bool                        currentArchetypeIsNative;
            private UnsafeFastList<int>.Enumerator currentEnumeratorNative;
            private BitMap.Enumerator           currentEnumerator;

            internal EntityEnumerator(Filter filter) {
                this.world      = filter.world;
                this.archetypes = filter.archetypes;
                this.current    = null;

                this.archetypeId    = 0;
                this.archetypeCount = this.archetypes.length;
                if (this.archetypeCount != 0) {
                    var currentArchetype = this.archetypes.GetValueByIndex(0);
                    
                    this.currentArchetypeIsNative = currentArchetype.usedInNative;
                    if (this.currentArchetypeIsNative) {
                        this.archetypeEntitiesNative = currentArchetype.entitiesNative;
                        this.currentEnumeratorNative = this.archetypeEntitiesNative.GetEnumerator();
                        
                        this.archetypeEntities = default;
                        this.currentEnumerator = default;
                    }
                    else {
                        this.archetypeEntities = currentArchetype.entities;
                        this.currentEnumerator = this.archetypeEntities.GetEnumerator();
                        
                        this.archetypeEntitiesNative = default;
                        this.currentEnumeratorNative = default;
                    }
                }
                else {
                    this.currentArchetypeIsNative = false;
                    
                    this.archetypeEntitiesNative = default;
                    this.currentEnumeratorNative = default;
                    
                    this.archetypeEntities = default;
                    this.currentEnumerator = default;
                }
            }

            public bool MoveNext() {
                if (this.archetypeCount == 1) {
                    if (this.currentArchetypeIsNative) {
                        if (this.currentEnumeratorNative.MoveNext()) {
                            this.current = this.world.entities[this.currentEnumeratorNative.current];
                            return true;
                        }
                    }
                    else {
                        if (this.currentEnumerator.MoveNext()) {
                            this.current = this.world.entities[this.currentEnumerator.current];
                            return true;
                        }
                    }
                    

                    return false;
                }

                if (this.archetypeId < this.archetypeCount) {
                    if (this.currentArchetypeIsNative) {
                        if (this.currentEnumeratorNative.MoveNext()) {
                            this.current = this.world.entities[this.currentEnumeratorNative.current];
                            return true;
                        }
                    }
                    else {
                        if (this.currentEnumerator.MoveNext()) {
                            this.current = this.world.entities[this.currentEnumerator.current];
                            return true;
                        }
                    }

                    while (++this.archetypeId < this.archetypeCount) {
                        var currentArchetype = this.archetypes.GetValueByIndex(this.archetypeId);
                        this.currentArchetypeIsNative = currentArchetype.usedInNative;

                        if (this.currentArchetypeIsNative) {
                            this.archetypeEntitiesNative = this.archetypes.GetValueByIndex(this.archetypeId).entitiesNative;
                            if (this.archetypeEntitiesNative.length > 0) {
                                this.currentEnumeratorNative = this.archetypeEntitiesNative.GetEnumerator();
                                this.currentEnumeratorNative.MoveNext();

                                this.current = this.world.entities[this.currentEnumeratorNative.current];
                                return true;
                            }
                        }
                        else {
                            this.archetypeEntities = this.archetypes.GetValueByIndex(this.archetypeId).entities;
                            if (this.archetypeEntities.count > 0) {
                                this.currentEnumerator = this.archetypeEntities.GetEnumerator();
                                this.currentEnumerator.MoveNext();

                                this.current = this.world.entities[this.currentEnumerator.current];
                                return true;
                            }
                        }
                        
                    }
                }

                return false;
            }

            [NotNull]
            public Entity Current => this.current;
        }
    }
}
