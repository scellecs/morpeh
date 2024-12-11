namespace Scellecs.Morpeh.Systems {
    using System;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Obsolete("Use ICleanupSystem instead.")]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class CleanupSystem : ScriptableObject, ICleanupSystem {
        public World World { get; set; }

        public abstract void OnAwake();

        public abstract void OnUpdate(float deltaTime);

        public virtual void Dispose() {
        }
    }
}