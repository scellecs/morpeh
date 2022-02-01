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
        private NativeFilter filter;
        
        [NativeDisableParallelForRestriction]
        private NativeCache<TNative> cache;

        [ReadOnly]
        public readonly int length;

        public NativeComponents(NativeFilter filter, NativeCache<TNative> cache) {
            this.filter   = filter;
            this.cache = cache;

            this.length = this.filter.Length;
        }

        public TNative this[int index] {
            get {
                var entityId          = this.filter[index];
                var componentPosition = this.cache.components.TryGetIndex(entityId);
                return this.cache.components.data[componentPosition];
            }
            set {
                var entityId          = this.filter[index];
                var componentPosition = this.cache.components.TryGetIndex(entityId);
                this.cache.components.data[componentPosition] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent(int index) {
            if (index < 0 || index >= this.length) return false;
            var entityId = this.filter[index];
            return entityId != -1 && this.cache.components.TryGetIndex(entityId) != -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref TNative GetComponent(int index, out bool exists) {
            exists = this.HasComponent(index);
            var entityId = this.filter[index];
            return ref this.cache.components.GetValueRefByKey(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref TNative GetComponent(int index) {
            var entityId = this.filter[index];
            return ref this.cache.components.GetValueRefByKey(entityId);
        }

        public void Dispose() {
            this.filter.Dispose();
        }
    }
}
#endif