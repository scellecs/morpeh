namespace morpeh.Core.NativeCollections {
    using Unity.Collections.LowLevel.Unsafe;

    public unsafe struct NativeArchetype {
        public                                            NativeFastList<int> entitiesBitMap;
        [NativeDisableUnsafePtrRestriction] public unsafe int*             lengthPtr;
    }
}