#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Collections;
    using JetBrains.Annotations;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FilterNode {
        public FastList<Filter> filters;
        public LongHashMap<FilterNode> nodes;
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FilterBuilder {
        internal World world;
        internal FilterBuilder parent;
        internal TypeInfo typeInfo;
        internal Filter.Mode mode;
        internal int level;
        internal TypeId includeHash;
        internal TypeId excludeHash;
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class Filter {
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public unsafe struct Chunk {
            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public int* entities;
            [ReadOnly]
            public int entitiesLength;
        }
        
        internal enum Mode {
            None    = 0,
            Include = 1,
            Exclude = 2
        }

        internal World world;

        internal FastList<Archetype> archetypes;
        internal HashSet<ArchetypeId> archetypeHashes;
        internal FastList<Chunk> chunks;

        internal FastList<TypeInfo> includedTypes;
        internal FastList<TypeInfo> excludedTypes;
        
        internal int archetypesLength;

        internal Filter(World world, FastList<TypeInfo> includedTypes, FastList<TypeInfo> excludedTypes) {
            this.world = world;

            this.archetypes = new FastList<Archetype>();
            this.archetypeHashes = new HashSet<ArchetypeId>();
            this.chunks  = new FastList<Chunk>();

            this.includedTypes = includedTypes;
            this.excludedTypes = excludedTypes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityEnumerator GetEnumerator() {
            this.world.ThreadSafetyCheck();
            if (this.archetypes.length == 0) {
                return default;
            }
            return new EntityEnumerator(this);
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct EntityEnumerator 
#if MORPEH_DEBUG
            : IEnumerator<Entity>
#endif
        {
            private readonly FastList<Archetype> archetypes;
            private readonly int                 archetypeCount;

            private int archetypeId;

            private Entity current;

            private World world;

            private BitMap archetypeEntities;
            private BitMap.Enumerator currentEnumerator;

            internal EntityEnumerator(Filter filter) {
                this.world      = filter.world;
                this.archetypes = filter.archetypes;
                this.current    = null;

                this.archetypeId    = 0;
                this.archetypeCount = this.archetypes.length;
                if (this.archetypeCount != 0) {
                    var currentArchetype = this.archetypes.data[0];
                    
                    this.archetypeEntities = currentArchetype.entities;
                    this.currentEnumerator = this.archetypeEntities.GetEnumerator();
                }
                else {
                    this.archetypeEntities = default;
                    this.currentEnumerator = default;
                }
#if MORPEH_DEBUG
                this.world.iteratorLevel++;
#endif
            }

            public bool MoveNext() {
                if (this.archetypeCount == 0) {
                    return false;
                }

                if (this.archetypeId < this.archetypeCount) {
                    if (this.currentEnumerator.MoveNext()) {
                        this.current = this.world.entities[this.currentEnumerator.current];
                        return true;
                    }

                    while (++this.archetypeId < this.archetypeCount) {
                        this.archetypeEntities = this.archetypes.data[this.archetypeId].entities;
                        if (this.archetypeEntities.count > 0) {
                            this.currentEnumerator = this.archetypeEntities.GetEnumerator();
                            this.currentEnumerator.MoveNext();

                            this.current = this.world.entities[this.currentEnumerator.current];
                            return true;
                        }
                    }
                }

                return false;
            }

            [NotNull]
            public Entity Current {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.current;
            }
#if MORPEH_DEBUG
            public void Dispose() {
                if (this.world != null) {
                    this.world.iteratorLevel--;
                }
            }

            object IEnumerator.Current => this.current;

            void IEnumerator.Reset() {}
#endif
        }
    }
}
