namespace Scellecs.Morpeh {
    using System;
    using Collections;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class Entity {
        internal World world;
        internal bool isDirty;
        internal bool isDisposed;
        
        internal VirtualArchetype previousVirtualArchetype;
        internal VirtualArchetype virtualArchetype;
        internal int indexInCurrentArchetype;

        internal EntityId entityId;
        
        [ShowInInspector]
        public EntityId ID => this.entityId;

        internal Entity() {
        }

        public override string ToString() {
            return $"Entity:{ID.ToString()}";
        }
    }
}
