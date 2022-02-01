#if UNITY_2019_1_OR_NEWER
namespace morpeh.Core.NativeCollections {
    using System;
    using System.Runtime.CompilerServices;
    using Morpeh;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;

    public struct NativeComponents<TNative> : IDisposable where TNative : unmanaged, IComponent {
        [ReadOnly]
        private NativeFilter entities;
        
        [NativeDisableParallelForRestriction]
        private NativeArray<TNative> components;

        [ReadOnly]
        public readonly int length;

        public NativeComponents(NativeFilter entities, NativeArray<TNative> components) {
            this.entities   = entities;
            this.components = components;

            this.length = this.entities.Length;
        }

        public TNative this[int index] {
            get => this.components[this.entities[index] + 1];
            set => this.components[this.entities[index] + 1] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent(int index) {
            if (index < 0 || index >= this.length) return false;
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