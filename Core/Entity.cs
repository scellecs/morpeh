namespace Scellecs.Morpeh {
    using System;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

#if !MORPEH_NON_SERIALIZED
    [Serializable]
#endif
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class Entity {
        [NonSerialized]
        internal World world;

        [SerializeField]
        internal int worldID;

        [SerializeField]
        internal bool isDirty;
        [SerializeField]
        internal bool isDisposed;

        [SerializeField]
        internal int previousArchetypeId;
        [SerializeField]
        internal int currentArchetypeId;

        [NonSerialized]
        internal Archetype currentArchetype;

        [NonSerialized]
        internal int indexInCurrentArchetype;

        [SerializeField]
        internal EntityId entityId;
        
        [ShowInInspector]
        public EntityId ID => this.entityId;

        internal Entity() {
        }
    }
}
