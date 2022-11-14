#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using Collections;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeIntHashMap<TNative> where TNative : unmanaged {
        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* lengthPtr;
        
        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* capacityPtr;
        
        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* capacityMinusOnePtr;
        
        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* lastIndexPtr;
        
        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* freeIndexPtr;

        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* buckets;
        
        [NativeDisableUnsafePtrRestriction]
        internal unsafe IntHashMapSlot* slots;
        
        [NativeDisableParallelForRestriction]
        [NativeDisableUnsafePtrRestriction]
        internal unsafe TNative* data;
    }
}
#endif