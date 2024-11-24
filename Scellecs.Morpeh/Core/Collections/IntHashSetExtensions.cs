namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class IntHashSetExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Add(this IntHashSet hashSet, in int value) {
            var rem = value & hashSet.capacityMinusOne;

            {
                var slots = hashSet.slots;
                var buckets = hashSet.buckets;
                int slotIndex;
                for (var i = buckets[rem] - 1; i >= 0; i = slots[slotIndex + 1]) {
                    slotIndex = i;
                    if (slots[slotIndex] - 1 == value) {
                        return false;
                    }
                }
            }

            int newIndex;
            if (hashSet.freeIndex >= 0) {
                newIndex = hashSet.freeIndex;
                hashSet.freeIndex = hashSet.slots[newIndex + 1];
            } else {
                if (hashSet.lastIndex == hashSet.capacity * 2) {
                    hashSet.Expand();
                    rem = value & hashSet.capacityMinusOne;
                }

                newIndex = hashSet.lastIndex;
                hashSet.lastIndex += 2;
            }

            {
                var slots = hashSet.slots;
                var buckets = hashSet.buckets;

                slots[newIndex] = value + 1;
                slots[newIndex + 1] = buckets[rem] - 1;

                buckets[rem] = newIndex + 1;
            }

            ++hashSet.length;
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Expand(this IntHashSet hashSet) {
            var newCapacityMinusOne = HashHelpers.GetCapacity(hashSet.length);
            var newCapacity = newCapacityMinusOne + 1;

            ArrayHelpers.Grow(ref hashSet.slots, newCapacity * 2);

            var newBuckets = new int[newCapacity];

            var slots = hashSet.slots;
            for (int i = 0, len = hashSet.lastIndex; i < len; i += 2) {
                var value = slots[i];
                var newResizeIndex = (value - 1) & newCapacityMinusOne;

                slots[i + 1] = newBuckets[newResizeIndex] - 1;
                newBuckets[newResizeIndex] = i + 1;
            }

            hashSet.buckets = newBuckets;
            hashSet.capacityMinusOne = newCapacityMinusOne;
            hashSet.capacity = newCapacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Remove(this IntHashSet hashSet, in int value) {
            var slots = hashSet.slots;
            var buckets = hashSet.buckets;
            var rem = value & hashSet.capacityMinusOne;

            int next;
            var num = -1;

            for (var i = buckets[rem] - 1; i >= 0; i = next) {
                if (slots[i] - 1 == value) {
                    if (num < 0) {
                        buckets[rem] = slots[i + 1] + 1;
                    }
                    else {
                        slots[num + 1] = slots[i + 1];
                    }

                    slots[i] = -1;
                    slots[i + 1] = hashSet.freeIndex;

                    if (--hashSet.length == 0) {
                        hashSet.lastIndex = 0;
                        hashSet.freeIndex = -1;
                    } else {
                        hashSet.freeIndex = i;
                    }

                    return true;
                }

                next = slots[i + 1];
                num = i;
            }

            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo(this IntHashSet hashSet, int[] array) {
            var num = 0;
            for (int i = 0, li = hashSet.lastIndex, len = hashSet.length; i < li && num < len; i+=2) {
                var v = hashSet.slots[i] - 1;
                if (v < 0) {
                    continue;
                }

                array[num] = v;
                ++num;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo(this IntHashSet from, IntHashSet to) {
            for (int i = 0, lastIndex = from.lastIndex; i < lastIndex; i += 2) {
                var v = from.slots[i] - 1;
                if (v < 0) {
                    continue;
                }

                to.Add(v);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(this IntHashSet hashSet, in int key) {
            var rem = key & hashSet.capacityMinusOne;

            int next;
            for (var i = hashSet.buckets[rem] - 1; i >= 0; i = next) {
                if (hashSet.slots[i] - 1 == key) {
                    return true;
                }

                next = hashSet.slots[i + 1];
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(this IntHashSet hashSet) {
            if (hashSet.lastIndex <= 0) {
                return;
            }

            Array.Clear(hashSet.slots, 0, hashSet.lastIndex);
            Array.Clear(hashSet.buckets, 0, hashSet.capacity);
            
            hashSet.lastIndex = 0;
            hashSet.length = 0;
            hashSet.freeIndex = -1;
        }
    }
}