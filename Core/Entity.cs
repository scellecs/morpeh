namespace Scellecs.Morpeh {
    using JetBrains.Annotations;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class Entity {
        internal World world;
        internal bool isDirty;
        internal bool isDisposed;
        
        internal long previousArchetype;
        internal long currentArchetype;
        internal int indexInCurrentArchetype;

        internal EntityId entityId;
        
        [ShowInInspector]
        public EntityId ID => this.entityId;

        internal Entity() { }

        public override string ToString() => $"Entity:{this.ID.ToString()}";
    }
}
