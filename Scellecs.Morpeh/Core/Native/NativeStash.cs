#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {

    public struct NativeStash<TNative> where TNative : unmanaged, IComponent {
        public NativeIntHashMap<TNative> components;
        public NativeWorld world;
    }
}
#endif