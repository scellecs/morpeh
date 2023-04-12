namespace Scellecs.Morpeh {
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Collections;
    using JetBrains.Annotations;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.IL2CPP.CompilerServices;
    
    public sealed class FilterBuilder {
        internal World world;
        internal FilterBuilder parent;
        internal int typeId;
        internal Filter.Mode mode;
        internal int level;
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class Filter : IEnumerable<Entity> {
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

        internal FastList<Filter>    childs;
        internal FastList<Archetype> archetypes;
        internal FastList<Chunk> chunks;

        internal FastList<int> includedTypeIds;
        internal FastList<int> excludedTypeIds;

        internal int  typeID;
        internal int  gen;
        internal Mode mode;

        internal Filter(World world) {
            this.world = world;

            this.childs     = new FastList<Filter>();
            this.archetypes = world.archetypes;

            this.typeID = -1;
            this.gen = 0;

            this.mode   = Mode.Include;
        }

        internal Filter(World world, FastList<int> includedTypeIds, FastList<int> excludedTypeIds) {
            this.world = world;

            this.childs     = new FastList<Filter>();
            this.archetypes = new FastList<Archetype>();
            this.archetypes = new FastList<Archetype>();
            this.chunks     = new FastList<Chunk>();

            this.includedTypeIds = includedTypeIds;
            this.excludedTypeIds = excludedTypeIds;

            this.gen = 0;

            this.world.filters.Add(this);
            this.AddArchetypes();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityEnumerator GetEnumerator() {
            this.world.ThreadSafetyCheck();
            return new EntityEnumerator(this);
        }

        IEnumerator<Entity> IEnumerable<Entity>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct EntityEnumerator : IEnumerator<Entity> {
            private readonly FastList<Archetype> archetypes;
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
                    var currentArchetype = this.archetypes.data[0];
                    
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
                        var currentArchetype = this.archetypes.data[this.archetypeId];
                        this.currentArchetypeIsNative = currentArchetype.usedInNative;

                        if (this.currentArchetypeIsNative) {
                            this.archetypeEntitiesNative = this.archetypes.data[this.archetypeId].entitiesNative;
                            if (this.archetypeEntitiesNative.length > 0) {
                                this.currentEnumeratorNative = this.archetypeEntitiesNative.GetEnumerator();
                                this.currentEnumeratorNative.MoveNext();

                                this.current = this.world.entities[this.currentEnumeratorNative.current];
                                return true;
                            }
                        }
                        else {
                            this.archetypeEntities = this.archetypes.data[this.archetypeId].entities;
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

            public void Reset() {
                this.current     = null;
                this.archetypeId = 0;
                if (this.archetypeCount != 0) {
                    var currentArchetype = this.archetypes.data[0];
                    
                    this.currentArchetypeIsNative = currentArchetype.usedInNative;
                    
                    if (currentArchetype.usedInNative) {
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

            [NotNull]
            public Entity Current => this.current;

            object IEnumerator.Current => this.current;

            public void Dispose() {
            }
        }
    }
}
