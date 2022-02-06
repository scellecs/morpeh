#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeFastListWrapper<TNative> where TNative : unmanaged {
        [NativeDisableUnsafePtrRestriction]
        public unsafe TNative* data;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* lengthPtr;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* capacityPtr;
    }
}
#endif