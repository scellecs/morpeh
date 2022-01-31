namespace morpeh.Core.NativeCollections {
    using Unity.Collections;

    public struct NativeFastList<TNative> where TNative : unmanaged {
        public        NativeArray<TNative> data;
        public unsafe int*                 lengthPtr;
        public unsafe int*                 capacityPtr;
    }
}