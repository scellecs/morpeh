namespace morpeh.Core.NativeCollections {
    using Morpeh;

    public struct NativeCache<TNative> where TNative : unmanaged, IComponent {
        public NativeIntHashMap<TNative> components;
    }
}