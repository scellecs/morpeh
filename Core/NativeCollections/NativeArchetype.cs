namespace morpeh.Core.NativeCollections {
    using Unity.Collections.LowLevel.Unsafe;

    public unsafe struct NativeArchetype {
        public                                            NativeBitMap entitiesBitMap;
        [NativeDisableUnsafePtrRestriction] public unsafe int*         lengthPtr;
    }
}