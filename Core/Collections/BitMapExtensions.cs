namespace Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static unsafe class BitMapExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Set(this BitMap bitmap, in int key) {
            var dataIndex = key >> BitMap.BITS_PER_FIELD_SHIFT;
            var bitIndex  = key - (dataIndex << BitMap.BITS_PER_FIELD_SHIFT);

            var rem = dataIndex & bitmap.capacityMinusOne;

            fixed (int* slotsPtr = &bitmap.slots[0])
            fixed (int* bucketsPtr = &bitmap.buckets[0])
            fixed (int* dataPtr = &bitmap.data[0]) {
                int* slot;
                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = *(slot + 1)) {
                    slot = slotsPtr + i;
                    if (*slot - 1 == dataIndex) {
                        var data    = *(dataPtr + (i >> 1));
                        var dataOld = data;

                        data |= 1 << bitIndex;

                        *(dataPtr + (i >> 1)) = data;
                        return data != dataOld;
                    }
                }
            }

            int slotIndex;
            if (bitmap.freeIndex >= 0) {
                slotIndex = bitmap.freeIndex;
                fixed (int* slotsPtr = &bitmap.slots[0]) {
                    bitmap.freeIndex = *(slotsPtr + slotIndex + 1);
                }
            }
            else {
                if (bitmap.lastIndex == bitmap.capacity << 1) {
                    var newCapacityMinusOne = HashHelpers.ExpandCapacitySmall(bitmap.length);
                    var newCapacity         = newCapacityMinusOne + 1;

                    ArrayHelpers.Grow(ref bitmap.slots, newCapacity << 1);
                    ArrayHelpers.Grow(ref bitmap.data, newCapacity);

                    var newBuckets = new int[newCapacity];

                    fixed (int* slotsPtr = &bitmap.slots[0])
                    fixed (int* newBucketsPtr = &newBuckets[0]) {
                        for (int i = 0, len = bitmap.lastIndex; i < len; i += 2) {
                            var slotPtr = slotsPtr + i;

                            var newResizeIndex   = (*slotPtr - 1) & newCapacityMinusOne;
                            var newCurrentBucket = newBucketsPtr + newResizeIndex;

                            *(slotPtr + 1) = *newCurrentBucket - 1;

                            *newCurrentBucket = i + 1;
                        }
                    }

                    bitmap.buckets          = newBuckets;
                    bitmap.capacity         = newCapacity;
                    bitmap.capacityMinusOne = newCapacityMinusOne;

                    rem = dataIndex & bitmap.capacityMinusOne;
                }

                slotIndex        =  bitmap.lastIndex;
                bitmap.lastIndex += 2;
            }

            fixed (int* slotsPtr = &bitmap.slots[0])
            fixed (int* bucketsPtr = &bitmap.buckets[0])
            fixed (int* dataPtr = &bitmap.data[0]) {
                var bucket = bucketsPtr + rem;
                var slot   = slotsPtr + slotIndex;

                *slot       = dataIndex + 1;
                *(slot + 1) = *bucket - 1;

                *(dataPtr + (slotIndex >> 1)) |= 1 << bitIndex;

                *bucket = slotIndex + 1;
            }

            ++bitmap.length;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unset(this BitMap bitmap, in int key) {
            var dataIndex = key >> BitMap.BITS_PER_FIELD_SHIFT;
            var bitIndex  = key - (dataIndex << BitMap.BITS_PER_FIELD_SHIFT);
            var rem       = dataIndex & bitmap.capacityMinusOne;

            fixed (int* slotsPtr = &bitmap.slots[0])
            fixed (int* bucketsPtr = &bitmap.buckets[0])
            fixed (int* dataPtr = &bitmap.data[0]) {
                int next;
                var num = -1;

                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = next) {
                    var slot     = slotsPtr + i;
                    var slotNext = slot + 1;

                    if (*slot - 1 == dataIndex) {
                        var data = *(dataPtr + (i >> 1));
                        data &= ~(1 << bitIndex);
                        if (data == 0) {
                            if (num < 0) {
                                *(bucketsPtr + rem) = *slotNext + 1;
                            }
                            else {
                                *(slotsPtr + num + 1) = *slotNext;
                            }
                            *slot     = -1;
                            *slotNext = bitmap.freeIndex;

                            if (--bitmap.length == 0) {
                                bitmap.lastIndex = 0;
                                bitmap.freeIndex = -1;
                            }
                            else {
                                bitmap.freeIndex = i;
                            }

                        }
                        *(dataPtr + (i >> 1)) = data;
                        return true;
                    }

                    next = *slotNext;
                    num  = i;
                }

                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Get(this BitMap bitmap, in int key) {
            var dataIndex = key >> BitMap.BITS_PER_FIELD_SHIFT;
            var bitIndex  = key - (dataIndex << BitMap.BITS_PER_FIELD_SHIFT);

            var rem = dataIndex & bitmap.capacityMinusOne;

            fixed (int* slotsPtr = &bitmap.slots[0])
            fixed (int* bucketsPtr = &bitmap.buckets[0])
            fixed (int* dataPtr = &bitmap.data[0]) {
                int* slot;
                for (var i = *(bucketsPtr + rem) - 1; i >= 0; i = *(slot + 1)) {
                    slot = slotsPtr + i;
                    if (*slot - 1 == dataIndex) {
                        return (*(dataPtr + (i >> 1)) & (1 << bitIndex)) > 0;
                    }
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(this BitMap bitMap) {
            if (bitMap.lastIndex <= 0) {
                return;
            }

            Array.Clear(bitMap.slots, 0, bitMap.lastIndex);
            Array.Clear(bitMap.buckets, 0, bitMap.capacity);
            Array.Clear(bitMap.data, 0, bitMap.capacity);

            bitMap.lastIndex = 0;
            bitMap.length    = 0;
            bitMap.freeIndex = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NumberOfTrailingZeros(int i) => lookupTrailingZeros[((uint)(i & -i) * 0x077CB531u) >> 27];

        private static readonly int[] lookupTrailingZeros = {
            0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8,
            31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };
    }
}