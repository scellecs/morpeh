namespace Morpeh {
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class Initializer : ScriptableObject, IInitializer {
        private World          world;
        private FilterProvider filter;

        public World World {
            get => this.world;
            set => this.world = value;
        }

        public FilterProvider Filter {
            get => this.filter;
            set => this.filter = value;
        }

        public abstract void OnAwake();

        public abstract void Dispose();
    }
}