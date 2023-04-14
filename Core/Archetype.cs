namespace Scellecs.Morpeh {
    using System;
    using Collections;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class Archetype {
        internal long[] typeIds;
        internal BitMap entities;
        internal UnsafeFastList<int> entitiesNative;
        internal FastList<Filter> filters;
        internal int length;
        internal int worldId;
        internal long id;
        internal bool usedInNative;

        internal World world;

       
        internal Archetype(long id, World world) {
            this.id             = id;
            this.length         = 0;
            this.entities       = new BitMap();
            this.usedInNative   = false;
            this.filters        = new FastList<Filter>();

            this.world = world;
        }
    }
}
