#if MORPEH_BURST
namespace Morpeh.Native {
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeArchetype {
        public NativeFastList<int> entitiesBitMap;

        [NativeDisableUnsafePtrRestriction] public unsafe int* lengthPtr;
    }
}
#endif
