namespace Morpeh {
    using UnityEngine;

    public abstract class Initializer : ScriptableObject, IInitializer {
        private World world;
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
        public abstract void OnStart();

        public abstract void Dispose();
    }
}