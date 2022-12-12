namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class IntHashMapExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Add<T>(this IntHashMap<T> hashMap, in int key, in T value, out int slotIndex) {
            var rem = key & hashMap.capacityMinusOne;

            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = hashMap.slots[i].next) {
                if (hashMap.slots[i].key - 1 == key) {
                    slotIndex = -1;
                    return false;
                }
            }

            if (hashMap.freeIndex >= 0) {
                slotIndex         = hashMap.freeIndex;
                hashMap.freeIndex = hashMap.slots[slotIndex].next;
            }
            else {
                if (hashMap.lastIndex == hashMap.capacity) {
                    var newCapacityMinusOne = HashHelpers.ExpandCapacity(hashMap.length);
                    var newCapacity         = newCapacityMinusOne + 1;

                    ArrayHelpers.Grow(ref hashMap.slots, newCapacity);
                    ArrayHelpers.Grow(ref hashMap.data, newCapacity);

                    var newBuckets = new int[newCapacity];

                    for (int i = 0, len = hashMap.lastIndex; i < len; ++i) {
                        ref var slot = ref hashMap.slots[i];

                        var newResizeIndex = (slot.key - 1) & newCapacityMinusOne;
                        slot.next = newBuckets[newResizeIndex] - 1;

                        newBuckets[newResizeIndex] = i + 1;
                    }

                    hashMap.buckets          = newBuckets;
                    hashMap.capacity         = newCapacity;
                    hashMap.capacityMinusOne = newCapacityMinusOne;

                    rem = key & hashMap.capacityMinusOne;
                }

                slotIndex = hashMap.lastIndex;
                ++hashMap.lastIndex;
            }

            ref var newSlot = ref hashMap.slots[slotIndex];

            newSlot.key  = key + 1;
            newSlot.next = hashMap.buckets[rem] - 1;

            hashMap.data[slotIndex] = value;

            hashMap.buckets[rem] = slotIndex + 1;

            ++hashMap.length;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Set<T>(this IntHashMap<T> hashMap, in int key, in T value, out int slotIndex) {
            var rem = key & hashMap.capacityMinusOne;

            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = hashMap.slots[i].next) {
                if (hashMap.slots[i].key - 1 == key) {
                    hashMap.data[i] = value;
                    slotIndex       = i;
                    return false;
                }
            }

            if (hashMap.freeIndex >= 0) {
                slotIndex         = hashMap.freeIndex;
                hashMap.freeIndex = hashMap.slots[slotIndex].next;
            }
            else {
                if (hashMap.lastIndex == hashMap.capacity) {
                    var newCapacityMinusOne = HashHelpers.ExpandCapacity(hashMap.length);
                    var newCapacity         = newCapacityMinusOne + 1;

                    ArrayHelpers.Grow(ref hashMap.slots, newCapacity);
                    ArrayHelpers.Grow(ref hashMap.data, newCapacity);

                    var newBuckets = new int[newCapacity];

                    for (int i = 0, len = hashMap.lastIndex; i < len; ++i) {
                        ref var slot           = ref hashMap.slots[i];
                        var     newResizeIndex = (slot.key - 1) & newCapacityMinusOne;
                        slot.next = newBuckets[newResizeIndex] - 1;

                        newBuckets[newResizeIndex] = i + 1;
                    }

                    hashMap.buckets          = newBuckets;
                    hashMap.capacity         = newCapacity;
                    hashMap.capacityMinusOne = newCapacityMinusOne;

                    rem = key & hashMap.capacityMinusOne;
                }

                slotIndex = hashMap.lastIndex;
                ++hashMap.lastIndex;
            }

            ref var newSlot = ref hashMap.slots[slotIndex];

            newSlot.key  = key + 1;
            newSlot.next = hashMap.buckets[rem] - 1;

            hashMap.data[slotIndex] = value;

            hashMap.buckets[rem] = slotIndex + 1;

            ++hashMap.length;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Remove<T>(this IntHashMap<T> hashMap, in int key, [CanBeNull] out T lastValue) {
            var rem = key & hashMap.capacityMinusOne;

            int next;
            int num = -1;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    if (num < 0) {
                        hashMap.buckets[rem] = slot.next + 1;
                    }
                    else {
                        hashMap.slots[num].next = slot.next;
                    }

                    lastValue       = hashMap.data[i];
                    hashMap.data[i] = default;

                    slot.key  = -1;
                    slot.next = hashMap.freeIndex;

                    --hashMap.length;
                    if (hashMap.length == 0) {
                        hashMap.lastIndex = 0;
                        hashMap.freeIndex = -1;
                    }
                    else {
                        hashMap.freeIndex = i;
                    }

                    return true;
                }

                next = slot.next;
                num  = i;
            }

            lastValue = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(this IntHashMap<T> hashMap, in int key) {
            var rem = key & hashMap.capacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    return true;
                }

                next = slot.next;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<T>(this IntHashMap<T> hashMap, in int key, [CanBeNull] out T value) {
            var rem = key & hashMap.capacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    value = hashMap.data[i];
                    return true;
                }

                next = slot.next;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueByKey<T>(this IntHashMap<T> hashMap, in int key) {
            var rem = key & hashMap.capacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    return hashMap.data[i];
                }

                next = slot.next;
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T TryGetValueRefByKey<T>(this IntHashMap<T> hashMap, in int key, out bool exist) {
            var rem = key & hashMap.capacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    exist = true;
                    return ref hashMap.data[i];
                }

                next = slot.next;
            }

            exist = false;
            return ref hashMap.data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetValueRefByKey<T>(this IntHashMap<T> hashMap, in int key) {
            var rem = key & hashMap.capacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    return ref hashMap.data[i];
                }

                next = slot.next;
            }

            return ref hashMap.data[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetValueByIndex<T>(this IntHashMap<T> hashMap, in int index) => hashMap.data[index];
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetValueRefByIndex<T>(this IntHashMap<T> hashMap, in int index) => ref hashMap.data[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetKeyByIndex<T>(this IntHashMap<T> hashMap, in int index) => hashMap.slots[index].key - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TryGetIndex<T>(this IntHashMap<T> hashMap, in int key) {
            var rem = key & hashMap.capacityMinusOne;

            int next;
            for (var i = hashMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref hashMap.slots[i];
                if (slot.key - 1 == key) {
                    return i;
                }

                next = slot.next;
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this IntHashMap<T> hashMap, T[] array) {
            int num = 0;
            for (int i = 0, li = hashMap.lastIndex; i < li && num < hashMap.length; ++i) {
                if (hashMap.slots[i].key - 1 < 0) {
                    continue;
                }

                array[num] = hashMap.data[i];
                ++num;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this IntHashMap<T> hashMap) {
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