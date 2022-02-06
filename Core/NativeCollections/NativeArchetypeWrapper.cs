#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeArchetypeWrapper {
        public                                            NativeFastListWrapper<int> entitiesBitMap;
        [NativeDisableUnsafePtrRestriction] public unsafe int*             lengthPtr;
    }
}
#endif