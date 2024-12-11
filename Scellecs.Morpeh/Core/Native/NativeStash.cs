#if MORPEH_BURST
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace Scellecs.Morpeh.Native {
    public struct NativeStash<TNative> where TNative : unmanaged, IComponent {
        [NativeDisableParallelForRestriction]
        [NativeDisableUnsafePtrRestriction]
        public unsafe TNative* data;

        [NativeDisableParallelForRestriction]
        [NativeDisableUnsafePtrRestriction]
        public unsafe TNative* empty;

        public NativeIntSlotMap map;
        public NativeWorld world;
    }
}
#endif