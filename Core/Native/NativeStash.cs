#if MORPEH_BURST
namespace Morpeh.Native {
    using Morpeh;
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeStash<TNative> where TNative : unmanaged, IComponent {
        [NativeDisableUnsafePtrRestriction]
        public NativeIntHashMap<TNative> components;
        
        [NativeDisableUnsafePtrRestriction]
        public NativeWorld world;
    }
}
#endif