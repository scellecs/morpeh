#if UNITY_2019_1_OR_NEWER
namespace morpeh.Core.Collections {
    using Morpeh;
    using Morpeh.Collections;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    [NativeContainer]
    public unsafe struct NativeIntHashMap<TNative> where TNative : unmanaged, IComponent {
        public int* lengthPtr;
        public int* capacityPtr;
        public int* capacityMinusOnePtr;
        public int* lastIndexPtr;
        public int* freeIndexPtr;
        
        public NativeArray<int>     buckets;
        public NativeArray<Slot>    slots;
        public NativeArray<TNative> data;
    }
}
#endif