#if MORPEH_BURST
namespace Morpeh.NativeCollections {
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