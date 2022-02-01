﻿#if UNITY_2019_1_OR_NEWER
namespace morpeh.Core.NativeCollections {
    using System.Runtime.CompilerServices;
    using Morpeh.Collections;
    using NativeIntHashMapJobs;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Jobs;

    public struct NativeIntHashMap<TNative> where TNative : unmanaged {
        [NativeDisableUnsafePtrRestriction] public unsafe int* lengthPtr;
        [NativeDisableUnsafePtrRestriction] public unsafe int* capacityPtr;
        [NativeDisableUnsafePtrRestriction] public unsafe int* capacityMinusOnePtr;
        [NativeDisableUnsafePtrRestriction] public unsafe int* lastIndexPtr;
        [NativeDisableUnsafePtrRestriction] public unsafe int* freeIndexPtr;
        
        public                                       NativeArray<int>            buckets;
        public                                       NativeArray<IntHashMapSlot> slots;
        [NativeDisableParallelForRestriction] public NativeArray<TNative>        data;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref TNative GetValueRefByKey(int key) {
            var rem = key & *this.capacityMinusOnePtr;

            int next;
            for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref UnsafeUtility.ArrayElementAsRef<IntHashMapSlot>(this.slots.GetUnsafePtr(), i);
                if (slot.key - 1 == key) {
                    return ref UnsafeUtility.ArrayElementAsRef<TNative>(this.data.GetUnsafePtr(), i);
                }

                next = slot.next;
            }

            return ref UnsafeUtility.ArrayElementAsRef<TNative>(this.data.GetUnsafePtr(), 0);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int TryGetIndex(in int key) {
            var rem = key & *this.capacityMinusOnePtr;

            int next;
            for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref UnsafeUtility.ArrayElementAsRef<IntHashMapSlot>(this.slots.GetUnsafePtr(), i);
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