namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
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
            
            var slotsPtr = bitmap.slots.ptr;
            var bucketsPtr = bitmap.buckets.ptr;
            var dataPtr = bitmap.data.ptr;

            for (var i = bucketsPtr[rem] - 1; i >= 0; i = slotsPtr[i + 1]) {
                if (slotsPtr[i] - 1 == dataIndex) {
                    var data    = dataPtr[i >> 1];
                    var dataOld = data;

                    data |= 1 << bitIndex;

                    dataPtr[i >> 1] = data;

                    var check = data != dataOld;
                    if (check) {
                        ++bitmap.count;
                    }
                    
                    return check;
                }
            }

            int slotIndex;
            if (bitmap.freeIndex >= 0) {
                slotIndex = bitmap.freeIndex;
                bitmap.freeIndex = slotsPtr[slotIndex + 1];
            }
            else {
                if (bitmap.lastIndex == bitmap.capacity << 1) {
                    bitmap.Resize(out rem, dataIndex);
                    slotsPtr = bitmap.slots.ptr;
                    bucketsPtr = bitmap.buckets.ptr;
                    dataPtr = bitmap.data.ptr;
                }

                slotIndex        =  bitmap.lastIndex;
                bitmap.lastIndex += 2;
            }

            slotsPtr[slotIndex] = dataIndex + 1;
            slotsPtr[slotIndex + 1] = bucketsPtr[rem] - 1;

            dataPtr[slotIndex >> 1] |= 1 << bitIndex;

            bucketsPtr[rem] = slotIndex + 1;

            ++bitmap.length;
            ++bitmap.count;
            return true;
        }
        
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Resize(this BitMap bitmap, out int rem, int dataIndex) {
            var newCapacityMinusOne = HashHelpers.GetCapacity(bitmap.length);
            var newCapacity         = newCapacityMinusOne + 1;

            bitmap.slots.Resize(newCapacity << 1);
            bitmap.data.Resize(newCapacity);

            var newBuckets = new IntPinnedArray(newCapacity);

            var slotsPtr = bitmap.slots.ptr;
            var newBucketsPtr = newBuckets.ptr;
            for (int i = 0, len = bitmap.lastIndex; i < len; i += 2) {
                var newResizeIndex   = (slotsPtr[i] - 1) & newCapacityMinusOne;
                slotsPtr[i + 1] = newBucketsPtr[newResizeIndex] - 1;
                newBucketsPtr[newResizeIndex] = i + 1;
            }

            bitmap.buckets.Dispose();
            bitmap.buckets          = newBuckets;
            bitmap.capacity         = newCapacity;
            bitmap.capacityMinusOne = newCapacityMinusOne;

            rem = dataIndex & bitmap.capacityMinusOne;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Unset(this BitMap bitmap, in int key) {
            var dataIndex = key >> BitMap.BITS_PER_FIELD_SHIFT;
            var bitIndex  = key - (dataIndex << BitMap.BITS_PER_FIELD_SHIFT);
            var rem       = dataIndex & bitmap.capacityMinusOne;
            
            var slotsPtr = bitmap.slots.ptr;
            var bucketsPtr = bitmap.buckets.ptr;
            var dataPtr = bitmap.data.ptr;

            int next;
            var num = -1;

            for (var i = bucketsPtr[rem] - 1; i >= 0; i = next) {
                if (slotsPtr[i] - 1 == dataIndex) {
                    var data    = dataPtr[i >> 1];
                    var dataOld = data;
                    data &= ~(1 << bitIndex);
                    if (data == 0) {
                        if (num < 0) {
                            bucketsPtr[rem] = slotsPtr[i + 1] + 1;
                        }
                        else {
                            slotsPtr[num + 1] = slotsPtr[i + 1];
                        }
                        slotsPtr[i]     = -1;
                        slotsPtr[i + 1] = bitmap.freeIndex;

                        if (--bitmap.length == 0) {
                            bitmap.lastIndex = 0;
                            bitmap.freeIndex = -1;
                        }
                        else {
                            bitmap.freeIndex = i;
                        }

                    }
                    dataPtr[i >> 1] = data;

                    var check = dataOld != data;
                    if (check) {
                        --bitmap.count;
                    }
                    
                    return check;
                }

                next = slotsPtr[i + 1];
                num  = i;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Get(this BitMap bitmap, in int key) {
            var dataIndex = key >> BitMap.BITS_PER_FIELD_SHIFT;
            var bitIndex  = key - (dataIndex << BitMap.BITS_PER_FIELD_SHIFT);

            var rem = dataIndex & bitmap.capacityMinusOne;

            var slotsPtr = bitmap.slots.ptr;
            var bucketsPtr = bitmap.buckets.ptr;
            var dataPtr = bitmap.data.ptr;
            
            for (var i = bucketsPtr[rem] - 1; i >= 0; i = slotsPtr[i + 1]) {
                if (slotsPtr[i] - 1 == dataIndex) {
                    return (dataPtr[i >> 1] & (1 << bitIndex)) != 0;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(this BitMap bitMap) {
            if (bitMap.lastIndex <= 0) {
                return;
            }

            bitMap.slots.Clear();
            bitMap.buckets.Clear();
            bitMap.data.Clear();

            bitMap.lastIndex = 0;
            bitMap.count     = 0;
            bitMap.length    = 0;
            bitMap.freeIndex = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int NumberOfTrailingZeros(int i) {
            switch (((uint)(i & -i) * 0x077CB531u) >> 27) {
                case 0:  return 0;
                case 1:  return 1;
                case 2:  return 28;
                case 3:  return 2;
                case 4:  return 29;
                case 5:  return 14;
                case 6:  return 24;
                case 7:  return 3;
                case 8:  return 30;
                case 9:  return 22;
                case 10: return 20;
                case 11: return 15;
                case 12: return 25;
                case 13: return 17;
                case 14: return 4;
                case 15: return 8;
                case 16: return 31;
                case 17: return 27;
                case 18: return 13;
                case 19: return 23;
                case 20: return 21;
                case 21: return 19;
                case 22: return 16;
                case 23: return 7;
                case 24: return 26;
                case 25: return 12;
                case 26: return 18;
                case 27: return 6;
                case 28: return 11;
                case 29: return 5;
                case 30: return 10;
                case 31: return 9;
            }
            return 0;
        }
    }
}