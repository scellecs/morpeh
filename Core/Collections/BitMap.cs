namespace Morpeh.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using morpeh.Core.NativeCollections;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.IL2CPP.CompilerServices;
    
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class BitMap : IEnumerable<int> {
        internal const int BITS_PER_BYTE        = 8;
        internal const int BITS_PER_FIELD       = BITS_PER_BYTE * sizeof(int);
        internal const int BITS_PER_FIELD_SHIFT = 5; //6 for long

        public int length;
        public int capacity;
        public int capacityMinusOne;
        public int lastIndex;
        public int freeIndex;

        public int[] buckets;
        public int[] data;
        public int[] slots;
        
        #if UNITY_2019_1_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe NativeBitMap AsNative() {
            var nativeBitMap = new NativeBitMap();
            
            fixed (int* lengthPtr = &this.length)
            fixed (int* capacityPtr = &this.capacity)
            fixed (int* capacityMinusOnePtr = &this.capacityMinusOne)
            fixed (int* lastIndexPtr = &this.lastIndex)
            fixed (int* freeIndexPtr = &this.freeIndex)
            fixed (int* bucketsPtr = this.buckets)
            fixed (int* dataPtr = this.data)
            fixed (int* slotsPtr = this.slots){
                nativeBitMap.lengthPtr           = lengthPtr;
                nativeBitMap.capacityPtr         = capacityPtr;
                nativeBitMap.capacityMinusOnePtr = capacityMinusOnePtr;
                nativeBitMap.lastIndexPtr        = lastIndexPtr;
                nativeBitMap.freeIndexPtr        = freeIndexPtr;
                nativeBitMap.data                = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(dataPtr, this.data.Length, Allocator.None);
                nativeBitMap.buckets             = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(bucketsPtr, this.buckets.Length, Allocator.None);
                nativeBitMap.slots               = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(slotsPtr, this.slots.Length, Allocator.None);
                
#if UNITY_EDITOR
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeBitMap.data, AtomicSafetyHandle.Create());
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeBitMap.buckets, AtomicSafetyHandle.Create());
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeBitMap.slots, AtomicSafetyHandle.Create());
#endif
            }

            return nativeBitMap;
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BitMap(in int capacity = 0) {
            this.lastIndex = 0;
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