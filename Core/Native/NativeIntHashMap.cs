#if MORPEH_BURST
namespace Morpeh.Native {
    using Collections;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeIntHashMap<TNative> where TNative : unmanaged {
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* lengthPtr;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* capacityPtr;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* capacityMinusOnePtr;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* lastIndexPtr;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* freeIndexPtr;

        [NativeDisableUnsafePtrRestriction]
        public unsafe int* buckets;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe IntHashMapSlot* slots;
        
        [NativeDisableParallelForRestriction]
        [NativeDisableUnsafePtrRestriction]
        public unsafe TNative* data;
    }
}
#endif