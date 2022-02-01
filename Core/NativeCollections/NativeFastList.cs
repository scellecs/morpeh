namespace morpeh.Core.NativeCollections {
    using Unity.Collections;

    public struct NativeFastList<TNative> where TNative : unmanaged {
        public unsafe TNative* data;
        public unsafe int*     lengthPtr;
        public unsafe int*     capacityPtr;
    }
}