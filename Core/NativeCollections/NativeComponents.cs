namespace morpeh.Core.Collections {
    using System;
    using Morpeh;
    using Unity.Collections;

    public struct NativeComponents<TNative> : IDisposable where TNative : unmanaged, IComponent {
        [ReadOnly]
        private NativeArray<int> entities;
        
        [NativeDisableParallelForRestriction]
        private NativeArray<TNative> components;

        [ReadOnly]
        public readonly int length;

        public NativeComponents(NativeArray<int> entities, NativeArray<TNative> components) {
            this.entities   = entities;
            this.components = components;

            this.length = this.entities.Length;
        }

        public TNative this[int index] {
            get => this.components[this.entities[index]];
            set => this.components[this.entities[index]] = value;
        }

        public void Dispose() {
            this.entities.Dispose();
            this.components.Dispose();
        }
    }
}