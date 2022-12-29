namespace Scellecs.Morpeh.Systems {
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class FixedUpdateSystem : ScriptableObject, IFixedSystem {
        public World World { get; set; }

        public abstract void OnAwake();

        public abstract void OnUpdate(float deltaTime);

        public virtual void Dispose() {
        }
    }
}