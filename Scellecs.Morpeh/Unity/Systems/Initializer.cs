namespace Scellecs.Morpeh.Systems {
    using System;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

#if !MORPEH_SUPPRESS_OBSOLETE
    [Obsolete("Use IInitializer instead.")]
#endif
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