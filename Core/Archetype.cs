namespace Morpeh {
    using System;
    using Collections;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class Archetype {
        [SerializeField]
        internal int[] typeIds;
        [SerializeField]
        internal bool isDirty;
        [SerializeField]
        internal FastList<int> entitiesBitMap;
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

        [NonSerialized]
        internal World world;

        internal Archetype(int id, int[] typeIds, int worldId) {
            this.id             = id;
            this.typeIds        = typeIds;
            this.length         = 0;
            this.entitiesBitMap = new FastList<int>();
            this.addTransfer    = new UnsafeIntHashMap<int>();
            this.removeTransfer = new UnsafeIntHashMap<int>();

            this.isDirty = false;
            this.worldId = worldId;

            this.Ctor();
        }
    }
}
