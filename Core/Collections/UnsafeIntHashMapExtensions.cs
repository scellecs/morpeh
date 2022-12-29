namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static unsafe class UnsafeIntHashMapExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Add<T>(this UnsafeIntHashMap<T> hashMap, in int key, in T value, out int slotIndex) where T : unmanaged {
            var rem = key & hashMap.capacityMinusOne;

            fixed (int* slotsPtr = &hashMap.slots[0])
            fixed (int* bucketsPtr = &hashMap.buckets[0]) {
                int* slot;
                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = *(slot + 1)) {
                    slot = slotsPtr + i;
                    if (*slot - 1 == key) {
                        slotIndex = -1;
                        return false;
                    }
                }
            }

            if (hashMap.freeIndex >= 0) {
                slotIndex = hashMap.freeIndex;
                fixed (int* s = &hashMap.slots[0]) {
                    hashMap.freeIndex = *(s + slotIndex + 1);
                }
            }
            else {
                if (hashMap.lastIndex == hashMap.capacity * 2) {
                    var newCapacityMinusOne = HashHelpers.ExpandCapacity(hashMap.length);
                    var newCapacity         = newCapacityMinusOne + 1;

                    ArrayHelpers.Grow(ref hashMap.slots, newCapacity * 2);
                    ArrayHelpers.Grow(ref hashMap.data, newCapacity);

                    var newBuckets = new int[newCapacity];

                    fixed (int* slotsPtr = &hashMap.slots[0])
                    fixed (int* bucketsPtr = &newBuckets[0]) {
                        for (int i = 0, len = hashMap.lastIndex; i < len; i += 2) {
                            var slotPtr = slotsPtr + i;

                            var newResizeIndex   = (*slotPtr - 1) & newCapacityMinusOne;
                            var newCurrentBucket = bucketsPtr + newResizeIndex;

                            *(slotPtr + 1) = *newCurrentBucket - 1;

                            *newCurrentBucket = i + 1;
                        }
                    }

                    hashMap.buckets          = newBuckets;
                    hashMap.capacity         = newCapacity;
                    hashMap.capacityMinusOne = newCapacityMinusOne;

                    rem = key & hashMap.capacityMinusOne;
                }

                slotIndex         =  hashMap.lastIndex;
                hashMap.lastIndex += 2;
            }

            fixed (int* slotsPtr = &hashMap.slots[0])
            fixed (int* bucketsPtr = &hashMap.buckets[0])
            fixed (T* dataPtr = &hashMap.data[0]) {
                var bucket = bucketsPtr + rem;
                var slot   = slotsPtr + slotIndex;

                *slot       = key + 1;
                *(slot + 1) = *bucket - 1;

                *(dataPtr + slotIndex / 2) = value;

                *bucket = slotIndex + 1;
            }

            ++hashMap.length;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Remove<T>(this UnsafeIntHashMap<T> hashMap, in int key, out T lastValue) where T : unmanaged {
            fixed (int* slotsPtr = &hashMap.slots[0])
            fixed (int* bucketsPtr = &hashMap.buckets[0])
            fixed (T* dataPtr = &hashMap.data[0]) {
                var rem = key & hashMap.capacityMinusOne;

                int next;
                var num = -1;

                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = next) {
                    var slot     = slotsPtr + i;
                    var slotNext = slot + 1;

                    if (*slot - 1 == key) {
                        if (num < 0) {
                            *(bucketsPtr + rem) = *slotNext + 1;
                        }
                        else {
                            *(slotsPtr + num + 1) = *slotNext;
                        }

                        var currentDataPtr = dataPtr + i / 2;
                        lastValue       = *currentDataPtr;
                        *currentDataPtr = default;

                        *slot     = -1;
                        *slotNext = hashMap.freeIndex;

                        if (--hashMap.length == 0) {
                            hashMap.lastIndex = 0;
                            hashMap.freeIndex = -1;
                        }
                        else {
                            hashMap.freeIndex = i;
                        }

                        return true;
                    }

                    next = *slotNext;
                    num  = i;
                }

                lastValue = default;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(this UnsafeIntHashMap<T> hashMap, in int key, out T value) where T : unmanaged {
            var rem = key & hashMap.capacityMinusOne;

            fixed (int* slotsPtr = &hashMap.slots[0])
            fixed (int* bucketsPtr = &hashMap.buckets[0])
            fixed (T* dataPtr = &hashMap.data[0]) {
                int* slot;
                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = *(slot + 1)) {
                    slot = slotsPtr + i;
                    if (*slot - 1 == key) {
                        value = *(dataPtr + i / 2);
                        return true;
                    }
                }
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueByKey<T>(this UnsafeIntHashMap<T> hashMap, in int key) where T : unmanaged {
            var rem = key & hashMap.capacityMinusOne;

            fixed (int* slotsPtr = &hashMap.slots[0])
            fixed (int* bucketsPtr = &hashMap.buckets[0])
            fixed (T* dataPtr = &hashMap.data[0]) {
                int next;
                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = next) {
                    if (*(slotsPtr + i) - 1 == key) {
                        return *(dataPtr + i / 2);
                    }

                    next = *(slotsPtr + i + 1);
                }
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueByIndex<T>(this UnsafeIntHashMap<T> hashMap, in int index) where T : unmanaged {
            fixed (T* d = &hashMap.data[0]) {
                return *(d + index / 2);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetKeyByIndex<T>(this UnsafeIntHashMap<T> hashMap, in int index) where T : unmanaged {
            fixed (int* d = &hashMap.slots[0]) {
                return *(d + index) - 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TryGetIndex<T>(this UnsafeIntHashMap<T> hashMap, in int key) where T : unmanaged {
            var rem = key & hashMap.capacityMinusOne;

            fixed (int* slotsPtr = &hashMap.slots[0])
            fixed (int* bucketsPtr = &hashMap.buckets[0]) {
                int* slot;
                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = *(slot + 1)) {
                    slot = slotsPtr + i;
                    if (*slot - 1 == key) {
                        return i;
                    }
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this UnsafeIntHashMap<T> hashMap) where T : unmanaged {
            if (hashMap.lastIndex <= 0) {
                return;
            }

            Array.Clear(hashMap.slots, 0, hashMap.lastIndex);
            Array.Clear(hashMap.buckets, 0, hashMap.capacity);
            Array.Clear(hashMap.data, 0, hashMap.capacity);

            hashMap.lastIndex = 0;
            hashMap.length    = 0;
            hashMap.freeIndex = -1;
        }
    }
}