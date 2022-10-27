namespace Morpeh.Native {
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeIntFastList {
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* data;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* lengthPtr;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* capacityPtr;
    }
}