namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class BitMap {
        internal const int BITS_PER_BYTE        = 8;
        internal const int BITS_PER_FIELD       = BITS_PER_BYTE * sizeof(int);
        internal const int BITS_PER_FIELD_SHIFT = 5; //5 for int, 6 for long

        public int count; //count of set bits
        public int length; //count of ints
        public int capacity;
        public int capacityMinusOne;
        public int lastIndex;
        public int freeIndex;

        public IntPinnedArray buckets;
        public IntPinnedArray data;
        public IntPinnedArray slots;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitMap(in int capacity = 0) {
            this.lastIndex = 0;
            this.count     = 0;
            this.length    = 0;
            this.freeIndex = -1;

            this.capacityMinusOne = HashHelpers.GetCapacity(capacity);
            this.capacity         = this.capacityMinusOne + 1;

            this.buckets = new IntPinnedArray(this.capacity);
            this.slots   = new IntPinnedArray(this.capacity << 1);
            this.data    = new IntPinnedArray(this.capacity);
        }

        ~BitMap() {
            this.buckets.Dispose();
            this.data.Dispose();
            this.slots.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.bitMap           = this;
            e.index            = default;
            e.current          = default;
            e.currentData      = default;
            e.currentDataIndex = default;
            return e;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public unsafe struct Enumerator {
            public BitMap bitMap;

            public int index;
            public int current;
            public int currentData;
            public int currentDataIndex;

            public bool MoveNext() {
                if (this.currentData != 0) {
                    this.current     =  this.currentDataIndex + BitMapExtensions.NumberOfTrailingZeros(this.currentData);
                    this.currentData &= this.currentData - 1;
                    return true;
                }

                {
                    var slotsPtr = this.bitMap.slots.ptr;
                    var dataPtr = this.bitMap.data.ptr;
                    for (; this.index < this.bitMap.lastIndex; this.index += 2) {
                        var dataIndex = slotsPtr[this.index] - 1;
                        if (dataIndex < 0) {
                            continue;
                        }

                        this.currentData      =  dataPtr[this.index >> 1];
                        this.currentDataIndex =  dataIndex * BITS_PER_FIELD;
                        this.current          =  this.currentDataIndex + BitMapExtensions.NumberOfTrailingZeros(this.currentData);
                        this.currentData      &= this.currentData - 1;

                        this.index += 2;
                        return true;
                    }
                }

                this.index       = this.bitMap.lastIndex + 1;
                this.current     = default;
                this.currentData = default;
                return false;
            }

            public int Current => this.current;
        }
    }
}