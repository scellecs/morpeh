#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {

    public struct NativeStash<TNative> where TNative : unmanaged, IComponent {
        internal NativeIntHashMap<TNative> components;
        internal NativeWorld world;
    }
}
#endif