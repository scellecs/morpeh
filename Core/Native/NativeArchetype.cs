#if MORPEH_BURST
namespace Morpeh.Native {
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeArchetype {
        public NativeIntFastList entitiesBitMap;

        [NativeDisableUnsafePtrRestriction] public unsafe int* lengthPtr;
    }
}
#endif
