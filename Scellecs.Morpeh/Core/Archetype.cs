using System.Collections.Generic;

namespace Scellecs.Morpeh {
    using Collections;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class Archetype {
        internal ArchetypeId id;
        internal int length;
        internal BitMap components;
        internal BitMap entities;
        internal HashSet<Filter> filters;
        
       
        internal Archetype(ArchetypeId id) {
            this.id = id;
            this.length = 0;
            this.components = new BitMap();
            this.entities = new BitMap();
            this.filters = new HashSet<Filter>();
        }
    }
}