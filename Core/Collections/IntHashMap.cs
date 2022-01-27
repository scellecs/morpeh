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
    public struct Slot {
        public int key;
        public int next;
    }
    
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class IntHashMap<T> : IEnumerable<int> {
        public int length;
        public int capacity;
        public int capacityMinusOne;
        public int lastIndex;
        public int freeIndex;

        public int[] buckets;

        public T[]    data;
        public Slot[] slots;
        
#if UNITY_2019_1_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe NativeIntHashMap<TNative> AsNative<TNative>() where TNative : unmanaged {
            var nativeIntHashMap = new NativeIntHashMap<TNative>();

            fixed (TNative* dataPtr = this.data as TNative[])
            fixed (int* bucketsPtr = this.buckets)
            fixed (Slot* slotsPtr = this.slots)
            fixed (int* lengthPtr = &this.length)
            fixed (int* capacityPtr = &this.capacity)
            fixed (int* capacityMinusOnePtr = &this.capacityMinusOne)
            fixed (int* lastIndexPtr = &this.lastIndex)
            fixed (int* freeIndexPtr = &this.freeIndex) {
                nativeIntHashMap.lengthPtr           = lengthPtr;
                nativeIntHashMap.capacityPtr         = capacityPtr;
                nativeIntHashMap.capacityMinusOnePtr = capacityMinusOnePtr;
                nativeIntHashMap.lastIndexPtr        = lastIndexPtr;
                nativeIntHashMap.freeIndexPtr        = freeIndexPtr;
                nativeIntHashMap.data                = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<TNative>(dataPtr, this.data.Length, Allocator.None);
                nativeIntHashMap.buckets             = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(bucketsPtr, this.buckets.Length, Allocator.None);
                nativeIntHashMap.slots               = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Slot>(slotsPtr, this.slots.Length, Allocator.None);
                
#if UNITY_EDITOR
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeIntHashMap.data, AtomicSafetyHandle.Create());
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeIntHashMap.buckets, AtomicSafetyHandle.Create());
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeIntHashMap.slots, AtomicSafetyHandle.Create());
#endif
            }

            return nativeIntHashMap;
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntHashMap(in int capacity = 0) {
            this.lastIndex = 0;
            this.length    = 0;
            this.freeIndex = -1;

            this.capacityMinusOne = HashHelpers.GetCapacity(capacity);
            this.capacity         = this.capacityMinusOne + 1;

            this.buckets = new int[this.capacity];
            this.slots   = new Slot[this.capacity];
            this.data    = new T[this.capacity];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.hashMap = this;
            e.index   = 0;
            e.current = default;
            return e;
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator : IEnumerator<int> {
            public IntHashMap<T> hashMap;

            public int index;
            public int current;

            public bool MoveNext() {
                for (; this.index < this.hashMap.lastIndex; ++this.index) {
                    ref var slot = ref this.hashMap.slots[this.index];
                    if (slot.key - 1 < 0) {
                        continue;
                    }

                    this.current = this.index;
                    ++this.index;

                    return true;
                }

                this.index   = this.hashMap.lastIndex + 1;
                this.current = default;
                return false;
            }

            public int Current => this.current;

            object IEnumerator.Current => this.current;

            void IEnumerator.Reset() {
                this.index   = 0;
                this.current = default;
            }

            public void Dispose() {
            }
        }
    }
}