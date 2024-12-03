namespace Scellecs.Morpeh.Systems {
    using System;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Obsolete("Use IInitializer instead.")]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class Initializer : ScriptableObject, IInitializer {
        public World World { get; set; }

        public abstract void OnAwake();

        public virtual void Dispose() {
        }
    }
}