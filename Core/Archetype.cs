namespace Scellecs.Morpeh {
    using System;
    using Collections;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class VirtualArchetype {
        public int level;
        public int typeId;
        public VirtualArchetype parent;
        public Archetype realArchetype;
        public IntHashMap<VirtualArchetype> map;
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class Archetype {
        [ShowInInspector]
        internal int[] typeIds;
        [ShowInInspector]
        internal BitMap entities;
        [ShowInInspector]
        internal UnsafeFastList<int> entitiesNative;
        [ShowInInspector]
        internal int length;
        [ShowInInspector]
        internal int id;
        [ShowInInspector]
        internal bool usedInNative;

        [NonSerialized]
        internal World world;

        internal Archetype(int id, int[] typeIds, World world) {
            this.id             = id;
            this.typeIds        = typeIds;
            this.length         = 0;
            this.entities       = new BitMap();
            this.entitiesNative = new UnsafeFastList<int>(0);
            this.usedInNative   = false;

            this.world = world;
        }
    }
}
