#if UNITY_2019_1_OR_NEWER
namespace morpeh.Core.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Morpeh;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    [NativeContainer]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent(int index) {
            if (index < 0 || index >= this.entities.Length) return false;
            return this.entities[index] != -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref TNative GetComponent(int index, out bool exists) {
            exists = this.HasComponent(index);
            return ref UnsafeUtility.ArrayElementAsRef<TNative>(this.components.GetUnsafePtr(), this.entities[index]);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref TNative GetComponent(int index) => ref UnsafeUtility.ArrayElementAsRef<TNative>(this.components.GetUnsafePtr(), this.entities[index]);

        public void Dispose() {
            this.entities.Dispose();
            this.components.Dispose();
        }
    }
}
#endif