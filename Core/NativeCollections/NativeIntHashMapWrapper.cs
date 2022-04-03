#if MORPEH_BURST
namespace Morpeh.NativeCollections {
    using System.Runtime.CompilerServices;
    using Collections;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeIntHashMapWrapper<TNative> where TNative : unmanaged {
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* lengthPtr;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* capacityPtr;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* capacityMinusOnePtr;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* lastIndexPtr;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe int* freeIndexPtr;

        [NativeDisableUnsafePtrRestriction]
        public unsafe int* buckets;
        
        [NativeDisableUnsafePtrRestriction]
        public unsafe IntHashMapSlot* slots;
        
        [NativeDisableParallelForRestriction]
        [NativeDisableUnsafePtrRestriction]
        public unsafe TNative* data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref TNative GetValueRefByKey(int key) {
            var rem = key & *this.capacityMinusOnePtr;

            int next;
            for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref UnsafeUtility.ArrayElementAsRef<IntHashMapSlot>(this.slots, i);
                if (slot.key - 1 == key) {
                    return ref UnsafeUtility.ArrayElementAsRef<TNative>(this.data, i);
                }

                next = slot.next;
            }

            return ref UnsafeUtility.ArrayElementAsRef<TNative>(this.data, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int TryGetIndex(in int key) {
            var rem = key & *this.capacityMinusOnePtr;

            int next;
            for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref UnsafeUtility.ArrayElementAsRef<IntHashMapSlot>(this.slots, i);
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