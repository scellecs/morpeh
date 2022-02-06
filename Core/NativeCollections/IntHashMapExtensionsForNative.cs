#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    using System.Runtime.CompilerServices;
    using Collections;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public static class IntHashMapExtensionsForNative {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe NativeIntHashMapWrapper<TNative> AsNative<TNative>(this IntHashMap<TNative> hashMap) where TNative : unmanaged {
            var nativeIntHashMap = new NativeIntHashMapWrapper<TNative>();
            
            fixed (int* lengthPtr = &hashMap.length)
            fixed (int* capacityPtr = &hashMap.capacity)
            fixed (int* capacityMinusOnePtr = &hashMap.capacityMinusOne)
            fixed (int* lastIndexPtr = &hashMap.lastIndex)
            fixed (int* freeIndexPtr = &hashMap.freeIndex)
            fixed (TNative* dataPtr = hashMap.data)
            fixed (int* bucketsPtr = hashMap.buckets)
            fixed (IntHashMapSlot* slotsPtr = hashMap.slots){
                nativeIntHashMap.lengthPtr           = lengthPtr;
                nativeIntHashMap.capacityPtr         = capacityPtr;
                nativeIntHashMap.capacityMinusOnePtr = capacityMinusOnePtr;
                nativeIntHashMap.lastIndexPtr        = lastIndexPtr;
                nativeIntHashMap.freeIndexPtr        = freeIndexPtr;
                nativeIntHashMap.data                = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<TNative>(dataPtr, hashMap.data.Length, Allocator.None);
                nativeIntHashMap.buckets             = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(bucketsPtr, hashMap.buckets.Length, Allocator.None);
                nativeIntHashMap.slots               = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<IntHashMapSlot>(slotsPtr, hashMap.slots.Length, Allocator.None);
                
#if UNITY_EDITOR
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeIntHashMap.data, AtomicSafetyHandle.Create());
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeIntHashMap.buckets, AtomicSafetyHandle.Create());
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeIntHashMap.slots, AtomicSafetyHandle.Create());
#endif
            }

            return nativeIntHashMap;
        }
    }
}
#endif