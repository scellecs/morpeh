#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeArchetype {
        public NativeFastList<int> entities;

        [NativeDisableUnsafePtrRestriction]
        public unsafe int* lengthPtr;
    }
}
#endif
