#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    using System;
    using System.Runtime.CompilerServices;
    using Morpeh;
    using Unity.Collections;

    public struct NativeComponents<TNative> : IDisposable where TNative : unmanaged, IComponent {
        [ReadOnly]
        private NativeFilterWrapper filterWrapper;
        
        [NativeDisableParallelForRestriction]
        private NativeCacheWrapper<TNative> cacheWrapper;

        [ReadOnly]
        public readonly int length;

        public NativeComponents(NativeFilterWrapper filterWrapper, NativeCacheWrapper<TNative> cacheWrapper) {
            this.filterWrapper   = filterWrapper;
            this.cacheWrapper = cacheWrapper;

            this.length = this.filterWrapper.Length;
        }

        internal TNative this[int index] {
            get {
                var entityId          = this.filterWrapper[index];
                var componentPosition = this.cacheWrapper.components.TryGetIndex(entityId);
                return this.cacheWrapper.components.data[componentPosition];
            }
            set {
                var entityId          = this.filterWrapper[index];
                var componentPosition = this.cacheWrapper.components.TryGetIndex(entityId);
                this.cacheWrapper.components.data[componentPosition] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent(int index) {
            if (index < 0 || index >= this.length) return false;
            var entityId = this.filterWrapper[index];
            return entityId != -1 && this.cacheWrapper.components.TryGetIndex(entityId) != -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TNative GetComponent(int index, out bool exists) {
            exists = this.HasComponent(index);
            var entityId = this.filterWrapper[index];
            return ref this.cacheWrapper.components.GetValueRefByKey(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TNative GetComponent(int index) {
            var entityId = this.filterWrapper[index];
            return ref this.cacheWrapper.components.GetValueRefByKey(entityId);
        }

        public void Dispose() {
            this.filterWrapper.Dispose();
        }
    }
}
#endif