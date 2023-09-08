namespace Scellecs.Morpeh {
    using System;
    using Collections;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

#if !MORPEH_NON_SERIALIZED
    [Serializable]
#endif
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed partial class Entity {
        [NonSerialized]
        internal World world;

        internal SortedBitMap components;

        [SerializeField]
        internal bool isDirty;
        [SerializeField]
        internal bool isDisposed;

        [SerializeField]
        internal long previousArchetype;
        [SerializeField]
        internal long currentArchetype;
        [SerializeField]
        internal int previousArchetypeLength;
        [SerializeField]
        internal int currentArchetypeLength;

        [SerializeField]
        internal EntityId entityId;
        
        [ShowInInspector]
        public EntityId ID => this.entityId;

        internal Entity() { }

        public override string ToString() => $"Entity:{ID.ToString()}";
    }
}
