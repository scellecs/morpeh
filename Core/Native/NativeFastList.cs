#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeFastList<TNative> where TNative : unmanaged {
        [NativeDisableUnsafePtrRestriction]
        internal unsafe TNative* data;
        
        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* lengthPtr;
        
        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* capacityPtr;
    }
}
#endif