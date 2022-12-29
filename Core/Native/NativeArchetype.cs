#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeArchetype {
        internal NativeFastList<int> entities;

        [NativeDisableUnsafePtrRestriction]
        internal unsafe int* lengthPtr;
    }
}
#endif
