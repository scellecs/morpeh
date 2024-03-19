namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static unsafe class UnsafeIntHashMapExtensions {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Resize<T>(this UnsafeIntHashMap<T> hashMap, out int rem, int key) where T : unmanaged {
            var newCapacityMinusOne = HashHelpers.GetCapacity(hashMap.length);
            var newCapacity         = newCapacityMinusOne + 1;

            hashMap.slots.Resize(newCapacity);
            hashMap.data.Resize(newCapacity);

            var newBuckets = new IntPinnedArray(newCapacity);

            {
                var slotsPtr = hashMap.slots.ptr;
                var bucketsPtr = newBuckets.ptr;
                for (int i = 0, len = hashMap.lastIndex; i < len; i++) {
                    var slot = slotsPtr + i;

                    var newResizeIndex   = (slot->key - 1) & newCapacityMinusOne;
                    var newCurrentBucket = bucketsPtr + newResizeIndex;

                    slot->next = *newCurrentBucket - 1;

                    *newCurrentBucket = i + 1;
                }
            }

            hashMap.buckets.Dispose();
            hashMap.buckets          = newBuckets;
            hashMap.capacity         = newCapacity;
            hashMap.capacityMinusOne = newCapacityMinusOne;

            rem = key & hashMap.capacityMinusOne;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Add<T>(this UnsafeIntHashMap<T> hashMap, in int key, in T value, out int slotIndex) where T : unmanaged {
            var rem = key & hashMap.capacityMinusOne;

            {
                var slotsPtr = hashMap.slots.ptr;
                var bucketsPtr = hashMap.buckets.ptr;
                IntHashMapSlot* slot;
                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = slot->next) {
                    slot = slotsPtr + i;
                    if (slot->key - 1 == key) {
                        slotIndex = -1;
                        return false;
                    }
                }
            }

            if (hashMap.freeIndex >= 0) {
                slotIndex = hashMap.freeIndex;
                hashMap.freeIndex = hashMap.slots.ptr[slotIndex].next;
            }
            else {
                if (hashMap.lastIndex == hashMap.capacity) {
                    hashMap.Resize(out rem, key);
                }

                slotIndex = hashMap.lastIndex;
                hashMap.lastIndex++;
            }

            {
                var slotsPtr = hashMap.slots.ptr;
                var bucketsPtr = hashMap.buckets.ptr;
                var dataPtr = hashMap.data.ptr;
                var bucket = bucketsPtr + rem;
                var slot   = slotsPtr + slotIndex;

                slot->key  = key + 1;
                slot->next = *bucket - 1;

                dataPtr[slotIndex] = value;

                *bucket = slotIndex + 1;
            }

            ++hashMap.length;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Remove<T>(this UnsafeIntHashMap<T> hashMap, in int key, out T lastValue) where T : unmanaged {
            {
                var slotsPtr = hashMap.slots.ptr;
                var bucketsPtr = hashMap.buckets.ptr;
                var dataPtr = hashMap.data.ptr;
                var rem = key & hashMap.capacityMinusOne;

                int next;
                var num = -1;

                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = next) {
                    var slot = slotsPtr + i;

                    if (slot->key - 1 == key) {
                        if (num < 0) {
                            *(bucketsPtr + rem) = slot->next + 1;
                        }
                        else {
                            (slotsPtr + num)->next = slot->next;
                        }

                        var currentDataPtr = dataPtr + i / 2;
                        lastValue       = *currentDataPtr;
                        *currentDataPtr = default;

                        slot->key     = -1;
                        slot->next = hashMap.freeIndex;

                        if (--hashMap.length == 0) {
                            hashMap.lastIndex = 0;
                            hashMap.freeIndex = -1;
                        }
                        else {
                            hashMap.freeIndex = i;
                        }

                        return true;
                    }

                    next = slot->next;
                    num  = i;
                }

                lastValue = default;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(this UnsafeIntHashMap<T> hashMap, in int key, out T value) where T : unmanaged {
            var rem = key & hashMap.capacityMinusOne;

            {
                var slotsPtr = hashMap.slots.ptr;
                var bucketsPtr = hashMap.buckets.ptr;
                var dataPtr = hashMap.data.ptr;
                IntHashMapSlot* slot;
                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = slot->next) {
                    slot = slotsPtr + i;
                    if (slot->key - 1 == key) {
                        value = dataPtr[i];
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

            {
                var slotsPtr = hashMap.slots.ptr;
                var bucketsPtr = hashMap.buckets.ptr;
                var dataPtr = hashMap.data.ptr;
                IntHashMapSlot* slot;
                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = slot->next) {
                    slot = slotsPtr + i;
                    if (slot->key - 1 == key) {
                        return dataPtr[i];
                    }
                }
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueByIndex<T>(this UnsafeIntHashMap<T> hashMap, in int index) where T : unmanaged => hashMap.data.ptr[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetKeyByIndex<T>(this UnsafeIntHashMap<T> hashMap, in int index) where T : unmanaged => (hashMap.slots.ptr + index)->key - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TryGetIndex<T>(this UnsafeIntHashMap<T> hashMap, in int key) where T : unmanaged {
            var rem = key & hashMap.capacityMinusOne;

            {
                var slotsPtr = hashMap.slots.ptr;
                var bucketsPtr = hashMap.buckets.ptr;
                IntHashMapSlot* slot;
                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = slot->next) {
                    slot = slotsPtr + i;
                    if (slot->key - 1 == key) {
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

            hashMap.slots.Clear();
            hashMap.buckets.Clear();
            hashMap.data.Clear();

            hashMap.lastIndex = 0;
            hashMap.length    = 0;
            hashMap.freeIndex = -1;
        }
    }
}