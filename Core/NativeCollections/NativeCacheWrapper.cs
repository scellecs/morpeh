#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    using Morpeh;

    public struct NativeCacheWrapper<TNative> where TNative : unmanaged, IComponent {
        public NativeIntHashMapWrapper<TNative> components;
    }
}
#endif