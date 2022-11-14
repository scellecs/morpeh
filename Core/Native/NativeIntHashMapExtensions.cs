#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Collections;
    using global::Morpeh;

    public static class NativeIntHashMapExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe NativeIntHashMap<TNative> AsNative<TNative>(this IntHashMap<TNative> hashMap) where TNative : unmanaged {
            var nativeIntHashMap = new NativeIntHashMap<TNative>();
            
            fixed (int* lengthPtr = &hashMap.length)
            fixed (int* capacityPtr = &hashMap.capacity)
            fixed (int* capacityMinusOnePtr = &hashMap.capacityMinusOne)
            fixed (int* lastIndexPtr = &hashMap.lastIndex)
            fixed (int* freeIndexPtr = &hashMap.freeIndex)
            fixed (TNative* dataPtr = &hashMap.data[0])
            fixed (int* bucketsPtr = &hashMap.buckets[0])
            fixed (IntHashMapSlot* slotsPtr = &hashMap.slots[0]){
                nativeIntHashMap.lengthPtr           = lengthPtr;
                nativeIntHashMap.capacityPtr         = capacityPtr;
                nativeIntHashMap.capacityMinusOnePtr = capacityMinusOnePtr;
                nativeIntHashMap.lastIndexPtr        = lastIndexPtr;
                nativeIntHashMap.freeIndexPtr        = freeIndexPtr;
                nativeIntHashMap.data                = dataPtr;
                nativeIntHashMap.buckets             = bucketsPtr;
                nativeIntHashMap.slots               = slotsPtr;
            }

            return nativeIntHashMap;
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ref TNative GetValueRefByKey<TNative>(this NativeIntHashMap<TNative> nativeIntHashMap, in int key) where TNative : unmanaged {
            var rem = key & *nativeIntHashMap.capacityMinusOnePtr;

            int next;
            for (var i = nativeIntHashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref UnsafeUtility.ArrayElementAsRef<IntHashMapSlot>(nativeIntHashMap.slots, i);
                if (slot.key - 1 == key) {
                    return ref UnsafeUtility.ArrayElementAsRef<TNative>(nativeIntHashMap.data, i);
                }

                next = slot.next;
            }

            return ref UnsafeUtility.ArrayElementAsRef<TNative>(nativeIntHashMap.data, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int TryGetIndex<TNative>(this NativeIntHashMap<TNative> nativeIntHashMap, in int key) where TNative : unmanaged {
            var rem = key & *nativeIntHashMap.capacityMinusOnePtr;

            int next;
            for (var i = nativeIntHashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref UnsafeUtility.ArrayElementAsRef<IntHashMapSlot>(nativeIntHashMap.slots, i);
                if (slot.key - 1 == key) {
                    return i;
                }

                next = slot.next;
            }

            return -1;
        }
    }
}
#endif