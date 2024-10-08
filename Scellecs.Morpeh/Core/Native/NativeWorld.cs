#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeWorld {
        public int identifier;
        public int generation;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe ushort* entitiesGens;

        [NativeDisableUnsafePtrRestriction]
        public unsafe int* entitiesCapacity;
    }
}
#endif