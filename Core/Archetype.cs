namespace Scellecs.Morpeh {
    using System;
    using Collections;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class Archetype {
        [ShowInInspector]
        internal BitMap entities;
        [ShowInInspector]
        internal UnsafeFastList<int> entitiesNative;
        [ShowInInspector]
        internal int length;
        [ShowInInspector]
        internal long id;
        [ShowInInspector]
        internal bool usedInNative;

        [NonSerialized]
        internal World world;

        internal Archetype(long id, World world) {
            this.id             = id;
            this.length         = 0;
            this.entities       = new BitMap();
            this.usedInNative   = false;

            this.world = world;
        }
    }
}
