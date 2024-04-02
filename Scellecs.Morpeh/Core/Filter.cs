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
    using System.Text;

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
            public Entity* entities;
            [ReadOnly]
            public int entitiesLength;
        }
        
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
        internal FastList<Chunk> chunks;

        internal TypeOffset[] includedOffsets;
        internal TypeOffset[] excludedOffsets;
        
        internal int id;

        internal Filter(World world, TypeOffset[] includedOffsets, TypeOffset[] excludedOffsets) {
            const int defaultArchetypesCapacity = 8;
            
            this.world = world;

            this.archetypes = new Archetype[defaultArchetypesCapacity];
            this.archetypesLength = 0;
            this.archetypesCapacity = defaultArchetypesCapacity;
            
            this.archetypeHashes = new LongHashMap<int>();
            this.chunks  = new FastList<Chunk>();

            this.includedOffsets = includedOffsets;
            this.excludedOffsets = excludedOffsets;

            this.id = world.filterCount++;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            
            sb.Append("Filter(");
            
            sb.Append("Include: ");
            foreach (var type in this.includedOffsets) {
                sb.Append(type);
                sb.Append(", ");
            }
            
            sb.Append("Exclude: ");
            foreach (var type in this.excludedOffsets) {
                sb.Append(type);
                sb.Append(", ");
            }
            
            sb.Append(")");
            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityEnumerator GetEnumerator() {
            this.world.ThreadSafetyCheck();

            return new EntityEnumerator(this.archetypes, this.archetypesLength
#if MORPEH_DEBUG
                , this.world
#endif
                );
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct EntityEnumerator
#if MORPEH_DEBUG
            : IEnumerator<Entity>
#endif
        {
            private Archetype[] archetypes;
            private int archetypeCount;
            
            private int archetypeIndex;
            private Archetype.Enumerator archetypeEnumerator;
            
#if MORPEH_DEBUG
            private World world;
#endif
            
            internal EntityEnumerator(Archetype[] archetypes, int archetypesCount
#if MORPEH_DEBUG
                , World world
#endif
                )
            {
                this.archetypes = archetypes;
                this.archetypeCount = archetypesCount;
                this.archetypeIndex = -1;
                
                this.archetypeEnumerator = default;
#if MORPEH_DEBUG
                this.world = world;
                ++this.world.iteratorLevel;
#endif
            }
            
            public Entity Current {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.archetypeEnumerator.Current;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() {
                while (this.archetypeIndex < this.archetypeCount) {
                    if (this.archetypeEnumerator.MoveNext()) {
                        return true;
                    }
                    
                    if (++this.archetypeIndex < this.archetypeCount) {
                        this.archetypeEnumerator = this.archetypes[this.archetypeIndex].GetEnumerator();
                    }
                }
                
                return false;
            }
            
#if MORPEH_DEBUG
            public void Dispose() {
                if (this.world != null) {
                    --this.world.iteratorLevel;
                }
            }
            
            object IEnumerator.Current => this.Current;
            
            public void Reset() { }
#endif
        }
    }
}
