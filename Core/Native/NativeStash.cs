#if MORPEH_BURST
namespace Morpeh.Native {
    using Morpeh;
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeStash<TNative> where TNative : unmanaged, IComponent {
        internal NativeIntHashMap<TNative> components;
        internal NativeWorld world;
    }
}
#endif