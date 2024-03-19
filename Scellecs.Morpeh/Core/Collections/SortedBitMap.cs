namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class SortedBitMap {
        internal const int BITS_PER_BYTE        = 8;
        internal const int BITS_PER_FIELD       = BITS_PER_BYTE * sizeof(int);
        internal const int BITS_PER_FIELD_SHIFT = 5; //5 for int, 6 for long

        public int count; //count of set bits
        public int length; //count of ints
        public int capacity;
        public int capacityMinusOne;
        public int lastIndex;
        public int freeIndex;
        public int head;

        public IntPinnedArray buckets;
        public IntPinnedArray data;
        //0 - key
        //1 - slotNext
        //2 - nodePrevious
        //3 - nodeNext
        public IntPinnedArray slots;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SortedBitMap(in int capacity = 0) {
            this.lastIndex = 0;
            this.count     = 0;
            this.length    = 0;
            this.freeIndex = -1;
            this.head      = -1;

            this.capacityMinusOne = HashHelpers.GetCapacity(capacity);
            this.capacity         = this.capacityMinusOne + 1;

            this.buckets = new IntPinnedArray(this.capacity);
            this.slots   = new IntPinnedArray(this.capacity << 2);
            this.data    = new IntPinnedArray(this.capacity);
        }

        ~SortedBitMap() {
            this.buckets.Dispose();
            this.data.Dispose();
            this.slots.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Enumerator GetEnumerator() {
            Enumerator e;
            e.bitMap           = this;
            e.index            = this.head;
            e.counter          = 1;
            e.current          = default;
            e.currentData      = e.index >= 0 ? this.data.ptr[this.head >> 2] : default;
            e.currentDataIndex = (this.slots.ptr[this.head] - 1) * BITS_PER_FIELD;
            return e;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public unsafe struct Enumerator {
            public SortedBitMap bitMap;

            public int index;
            public int counter;
            public int current;
            public int currentData;
            public int currentDataIndex;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() {
                if (this.currentData != 0) {
                    this.current     =  this.currentDataIndex + SortedBitMapExtensions.NumberOfTrailingZeros(this.currentData);
                    this.currentData &= this.currentData - 1;
                    return true;
                }

                return this.NextData();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private bool NextData() {
                var slotsPtr = this.bitMap.slots.ptr;
                var dataPtr = this.bitMap.data.ptr;
                
                if (this.counter < this.bitMap.length) {
                    this.index = slotsPtr[this.index + 3];
                    var dataIndex = slotsPtr[this.index] - 1;
                    
                    this.currentData      =  dataPtr[this.index >> 2];
                    this.currentDataIndex =  dataIndex * BITS_PER_FIELD;
                    this.current          =  this.currentDataIndex + SortedBitMapExtensions.NumberOfTrailingZeros(this.currentData);
                    this.currentData      &= this.currentData - 1;

                    this.counter++;
                    return true;
                }
                
                this.index       = default;
                this.current     = default;
                this.currentData = default;
                return false;
            }

            public int Current => this.current;
        }
    }
}