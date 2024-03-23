using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh {
    using Collections;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class Archetype {
        internal ArchetypeId id;
        
        internal PinnedArray<Entity> entities;
        internal int length;
        
        internal BitMap components;
        internal IntHashMap<Filter> filters;
       
        internal Archetype(ArchetypeId id) {
            this.id = id;
            
            this.entities = new PinnedArray<Entity>(16);
            this.length = 0;
            
            this.components = new BitMap();
            this.filters = new IntHashMap<Filter>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            return new Enumerator {
                entities = this.entities,
                length = this.length,
                index = -1,
            };
        }
        
        internal struct Enumerator {
            internal PinnedArray<Entity> entities;
            internal int length;
            internal int index;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() {
                return ++this.index < this.length;
            }
            
            public Entity Current {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.entities[this.index];
            }
        }
    }
}
