#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    using Morpeh;

    public struct NativeCache<TNative> where TNative : unmanaged, IComponent {
        public NativeIntHashMap<TNative> components;
    }
}
#endif