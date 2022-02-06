#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    using Morpeh;
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeCacheWrapper<TNative> where TNative : unmanaged, IComponent {
        [NativeDisableUnsafePtrRestriction]
        public NativeIntHashMapWrapper<TNative> components;
    }
}
#endif