using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh {
    using Collections;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class Archetype {
        internal ArchetypeHash hash;
        
        internal PinnedArray<Entity> entities;
        internal int length;
        
        internal IntHashSet components;
        internal IntHashMap<Filter> filters;
       
        internal Archetype(ArchetypeHash hash) {
            this.hash = hash;
            
            this.entities = new PinnedArray<Entity>(16);
            this.length = 0;
            
            this.components = new IntHashSet(8);
            this.filters = new IntHashMap<Filter>();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            
            e.entities = this.entities.data;
            e.index = -1;
            e.length = this.length;
            
            return e;
        }
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator {
            internal Entity[] entities;
            internal int index;
            internal int length;
            
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
