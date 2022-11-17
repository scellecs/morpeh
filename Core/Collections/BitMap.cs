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
    public sealed class BitMap : IEnumerable<int> {
        internal const int BITS_PER_BYTE        = 8;
        internal const int BITS_PER_FIELD       = BITS_PER_BYTE * sizeof(int);
        internal const int BITS_PER_FIELD_SHIFT = 5; //6 for long

        public int count; //count of set bits
        public int length; //count of ints
        public int capacity;
        public int capacityMinusOne;
        public int lastIndex;
        public int freeIndex;

        public int[] buckets;
        public int[] data;
        public int[] slots;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitMap(in int capacity = 0) {
            this.lastIndex = 0;
            this.count     = 0;
            this.length    = 0;
            this.freeIndex = -1;

            this.capacityMinusOne = HashHelpers.GetCapacity(capacity);
            this.capacity         = this.capacityMinusOne + 1;

            this.buckets = new int[this.capacity];
            this.slots   = new int[this.capacity << 1];
            this.data    = new int[this.capacity];
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

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
        public unsafe struct Enumerator : IEnumerator<int> {
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

                fixed (int* slotsPtr = &this.bitMap.slots[0])
                fixed (int* dataPtr = &this.bitMap.data[0]) {
                    for (; this.index < this.bitMap.lastIndex; this.index += 2) {
                        var dataIndex = *(slotsPtr + this.index) - 1;
                        if (dataIndex < 0) {
                            continue;
                        }

                        this.currentData      =  *(dataPtr + (this.index >> 1));
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

            object IEnumerator.Current => this.current;

            void IEnumerator.Reset() {
                this.index            = default;
                this.current          = default;
                this.currentData      = default;
                this.currentDataIndex = default;
            }

            public void Dispose() {
            }
        }
    }
}