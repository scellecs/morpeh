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
        internal LongHashMap<int> removeTransfer;
        internal LongHashMap<int> addTransfer;
        internal int length;
        internal int worldId;
        internal int id;
        internal bool usedInNative;

        internal World world;

        internal Archetype(int id, long[] typeIds, int worldId) {
            this.id             = id;
            this.typeIds        = typeIds;
            this.length         = 0;
            this.entities       = new BitMap();
            this.entitiesNative = new UnsafeFastList<int>(0);
            this.addTransfer    = new LongHashMap<int>();
            this.removeTransfer = new LongHashMap<int>();
            this.usedInNative   = false;

            this.worldId = worldId;

            this.Ctor();
        }
    }
}
