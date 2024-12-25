using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Scellecs.Morpeh.Native
{
    public struct NativeDisposableStash<TNative> where TNative : unmanaged, IDisposableComponent {
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