namespace morpeh.Core.NativeCollections {
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public unsafe struct NativeBitMap {
        internal const int BITS_PER_BYTE        = 8;
        internal const int BITS_PER_FIELD       = BITS_PER_BYTE * sizeof(int);
        internal const int BITS_PER_FIELD_SHIFT = 5; //6 for long
        
        [NativeDisableUnsafePtrRestriction] public int* lengthPtr;
        [NativeDisableUnsafePtrRestriction] public int* capacityPtr;
        [NativeDisableUnsafePtrRestriction] public int* capacityMinusOnePtr;
        [NativeDisableUnsafePtrRestriction] public int* lastIndexPtr;
        [NativeDisableUnsafePtrRestriction] public int* freeIndexPtr;

        [NativeDisableUnsafePtrRestriction] public int*  buckets;
        [NativeDisableUnsafePtrRestriction] public int*  data;
        [NativeDisableUnsafePtrRestriction] public int*  slots;
        [NativeDisableUnsafePtrRestriction] public byte* density;
    }
}