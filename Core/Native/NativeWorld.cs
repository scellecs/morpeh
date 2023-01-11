#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeWorld {
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* entitiesGens;

        [NativeDisableUnsafePtrRestriction]
        public unsafe int* entitiesCapacity;
    }
}
#endif