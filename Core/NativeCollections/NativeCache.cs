#if MORPEH_BURST
namespace Morpeh.NativeCollections {
    using System.Runtime.CompilerServices;
    using Morpeh;
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeCache<TNative> where TNative : unmanaged, IComponent {
        [NativeDisableUnsafePtrRestriction]
        public NativeIntHashMapWrapper<TNative> components;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent(int entityId) {
            return entityId != -1 && this.components.TryGetIndex(entityId) != -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TNative GetComponent(int entityId, out bool exists) {
            exists = this.HasComponent(entityId);
            return ref this.components.GetValueRefByKey(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TNative GetComponent(int entityId) {
            return ref this.components.GetValueRefByKey(entityId);
        }
    }
}
#endif