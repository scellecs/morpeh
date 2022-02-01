#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    public struct NativeFastList<TNative> where TNative : unmanaged {
        public unsafe TNative* data;
        public unsafe int*     lengthPtr;
        public unsafe int*     capacityPtr;
    }
}
#endif