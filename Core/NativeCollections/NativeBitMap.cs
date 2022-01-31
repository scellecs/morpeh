namespace morpeh.Core.NativeCollections {
    using Unity.Collections;

    public struct NativeBitMap {
        internal const int BITS_PER_BYTE        = 8;
        internal const int BITS_PER_FIELD       = BITS_PER_BYTE * sizeof(int);
        internal const int BITS_PER_FIELD_SHIFT = 5; //6 for long
        
        public unsafe int* lengthPtr;
        public unsafe int* capacityPtr;
        public unsafe int* capacityMinusOnePtr;
        public unsafe int* lastIndexPtr;
        public unsafe int* freeIndexPtr;

        public NativeArray<int> buckets;
        public NativeArray<int> data;
        public NativeArray<int> slots;
    }
}