namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static unsafe class IntHashSetExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Add(this IntHashSet hashSet, in int value) {
            var rem = value & hashSet.capacityMinusOne;

            fixed (int* slotsPtr = &hashSet.slots[0])
            fixed (int* bucketsPtr = &hashSet.buckets[0]) {
                int* slot;
                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = *(slot + 1)) {
                    slot = slotsPtr + i;
                    if (*slot - 1 == value) {
                        return false;
                    }
                }
            }

            int newIndex;
            if (hashSet.freeIndex >= 0) {
                newIndex = hashSet.freeIndex;
                fixed (int* s = &hashSet.slots[0]) {
                    hashSet.freeIndex = *(s + newIndex + 1);
                }
            }
            else {
                if (hashSet.lastIndex == hashSet.capacity * 2) {
                    var newCapacityMinusOne = HashHelpers.ExpandCapacity(hashSet.length);
                    var newCapacity         = newCapacityMinusOne + 1;

                    ArrayHelpers.Grow(ref hashSet.slots, newCapacity * 2);

                    var newBuckets = new int[newCapacity];

                    fixed (int* slotsPtr = &hashSet.slots[0])
                    fixed (int* bucketsPtr = &newBuckets[0]) {
                        for (int i = 0, len = hashSet.lastIndex; i < len; i += 2) {
                            var slotPtr = slotsPtr + i;

                            var newResizeIndex   = (*slotPtr - 1) & newCapacityMinusOne;
                            var newCurrentBucket = bucketsPtr + newResizeIndex;

                            *(slotPtr + 1) = *newCurrentBucket - 1;

                            *newCurrentBucket = i + 1;
                        }
                    }

                    hashSet.buckets          = newBuckets;
                    hashSet.capacityMinusOne = newCapacityMinusOne;
                    hashSet.capacity         = newCapacity;

                    rem = value & newCapacityMinusOne;
                }

                newIndex          =  hashSet.lastIndex;
                hashSet.lastIndex += 2;
            }

            fixed (int* slotsPtr = &hashSet.slots[0])
            fixed (int* bucketsPtr = &hashSet.buckets[0]) {
                var bucket = bucketsPtr + rem;
                var slot   = slotsPtr + newIndex;

                *slot       = value + 1;
                *(slot + 1) = *bucket - 1;

                *bucket = newIndex + 1;
            }

            ++hashSet.length;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Remove(this IntHashSet hashSet, in int value) {
            fixed (int* slotsPtr = &hashSet.slots[0])
            fixed (int* bucketsPtr = &hashSet.buckets[0]) {
                var rem = value & hashSet.capacityMinusOne;

                int next;
                var num = -1;

                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = next) {
                    var slot     = slotsPtr + i;
                    var slotNext = slot + 1;

                    if (*slot - 1 == value) {
                        if (num < 0) {
                            *(bucketsPtr + rem) = *slotNext + 1;
                        }
                        else {
                            *(slotsPtr + num + 1) = *slotNext;
                        }

                        *slot     = -1;
                        *slotNext = hashSet.freeIndex;

                        if (--hashSet.length == 0) {
                            hashSet.lastIndex = 0;
                            hashSet.freeIndex = -1;
                        }
                        else {
                            hashSet.freeIndex = i;
                        }

                        return true;
                    }

                    next = *slotNext;
                    num  = i;
                }

                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo(this IntHashSet hashSet, int[] array) {
            fixed (int* slotsPtr = &hashSet.slots[0]) {
                var num = 0;
                for (int i = 0, li = hashSet.lastIndex, len = hashSet.length; i < li && num < len; ++i) {
                    var v = *(slotsPtr + i) - 1;
                    if (v < 0) {
                        continue;
                    }

                    array[num] = v;
                    ++num;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(this IntHashSet hashSet, in int key) {
            fixed (int* slotsPtr = &hashSet.slots[0])
            fixed (int* bucketsPtr = &hashSet.buckets[0]) {
                var rem = key & hashSet.capacityMinusOne;

                int next;
                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = next) {
                    var slot = slotsPtr + i;
                    if (*slot - 1 == key) {
                        return true;
                    }

                    next = *(slot + 1);
                }

                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(this IntHashSet hashSet) {
            if (hashSet.lastIndex <= 0) {
                return;
            }

            Array.Clear(hashSet.slots, 0, hashSet.lastIndex);
            Array.Clear(hashSet.buckets, 0, hashSet.capacityMinusOne);
            hashSet.lastIndex = 0;
            hashSet.length    = 0;
            hashSet.freeIndex = -1;
        }
    }
}