#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static unsafe class NativeIntHashMapExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static NativeIntHashMap<TNative> AsNative<TNative>(this IntHashMap<TNative> hashMap) where TNative : unmanaged {
            var nativeIntHashMap = new NativeIntHashMap<TNative>();
            
            fixed (int* lengthPtr = &hashMap.length)
            fixed (int* capacityPtr = &hashMap.capacity)
            fixed (int* capacityMinusOnePtr = &hashMap.capacityMinusOne)
            fixed (int* lastIndexPtr = &hashMap.lastIndex)
            fixed (int* freeIndexPtr = &hashMap.freeIndex)
            fixed (TNative* dataPtr = &hashMap.data[0]) {
                nativeIntHashMap.lengthPtr           = lengthPtr;
                nativeIntHashMap.capacityPtr         = capacityPtr;
                nativeIntHashMap.capacityMinusOnePtr = capacityMinusOnePtr;
                nativeIntHashMap.lastIndexPtr        = lastIndexPtr;
                nativeIntHashMap.freeIndexPtr        = freeIndexPtr;
                nativeIntHashMap.data                = dataPtr;
                nativeIntHashMap.buckets             = hashMap.buckets.ptr;
                nativeIntHashMap.slots               = hashMap.slots.ptr;
            }

            return nativeIntHashMap;
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative GetValueRefByKey<TNative>(this NativeIntHashMap<TNative> nativeIntHashMap, in int key) where TNative : unmanaged {
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
        public static TNative GetValueByIndex<TNative>(this NativeIntHashMap<TNative> hashMap, in int index) where TNative : unmanaged => *(hashMap.data + index);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative GetValueRefByIndex<TNative>(this NativeIntHashMap<TNative> hashMap, in int index) where TNative : unmanaged => ref *(hashMap.data + index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TryGetIndex<TNative>(this NativeIntHashMap<TNative> nativeIntHashMap, in int key) where TNative : unmanaged {
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