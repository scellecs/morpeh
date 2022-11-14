namespace Scellecs.Morpeh {
    using System;
    using Collections;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

#if !MORPEH_NON_SERIALIZED
    [Serializable]
#endif
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class Archetype {
        [SerializeField]
        internal int[] typeIds;
        [SerializeField]
        internal BitMap entities;
        [SerializeField]
        internal FastList<int> entitiesNative;
        [NonSerialized]
        internal FastList<Filter> filters;
        [SerializeField]
        internal UnsafeIntHashMap<int> removeTransfer;
        [SerializeField]
        internal UnsafeIntHashMap<int> addTransfer;
        [SerializeField]
        internal int length;
        [SerializeField]
        internal int worldId;
        [SerializeField]
        internal int id;
        [SerializeField]
        internal bool usedInNative;

        [NonSerialized]
        internal World world;

        internal Archetype(int id, int[] typeIds, int worldId) {
            this.id             = id;
            this.typeIds        = typeIds;
            this.length         = 0;
            this.entities       = new BitMap();
            this.addTransfer    = new UnsafeIntHashMap<int>();
            this.removeTransfer = new UnsafeIntHashMap<int>();
            this.usedInNative   = false;

            this.worldId = worldId;

            this.Ctor();
        }
    }
}
