#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
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
        internal TypeHash includeHash;
        internal TypeHash excludeHash;
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

        internal int[] includedTypeIds;
        internal BitSet includedTypeIdsLookup;
        
        internal int[] excludedTypeIds;
        internal BitSet excludedTypeIdsLookup;
        
        internal int id;

        internal Filter(World world, int[] includedTypeIds, int[] excludedTypeIds) {
            const int defaultArchetypesCapacity = 8;
            
            this.world = world;

            this.archetypes = new Archetype[defaultArchetypesCapacity];
            this.archetypesLength = 0;
            this.archetypesCapacity = defaultArchetypesCapacity;
            
            this.archetypeHashes = new LongHashMap<int>();
            this.chunks  = new FastList<Chunk>();

            this.includedTypeIds = includedTypeIds;
            this.includedTypeIdsLookup = new BitSet(includedTypeIds);
            
            this.excludedTypeIds = excludedTypeIds;
            this.excludedTypeIdsLookup = new BitSet(excludedTypeIds);

            this.id = world.filterCount++;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            
            sb.Append("Filter(");
            
            sb.Append("Include: ");
            foreach (var type in this.includedTypeIds) {
                sb.Append(type);
                sb.Append(", ");
            }
            
            sb.Append("Exclude: ");
            foreach (var type in this.excludedTypeIds) {
                sb.Append(type);
                sb.Append(", ");
            }
            
            sb.Append(")");
            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            this.world.ThreadSafetyCheck();
            var e = default(Enumerator);
            
            e.archetypes = this.archetypes;
            e.archetypeCount = this.archetypesLength;
            
            e.archetypeIndex = -1;
            e.archetypeEnumerator = default;
            
#if MORPEH_DEBUG
            ++this.world.iteratorLevel;
            e.world = world;
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
            internal Archetype[] archetypes;
            internal int archetypeCount;
            
            internal int archetypeIndex;
            internal Archetype.Enumerator archetypeEnumerator;
            
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
        }
    }
}
