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
    public struct IntHashMapSlot {
        public int key;
        public int next;
    }
    
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class IntHashMap<T> {
        public int length;
        public int capacity;
        public int capacityMinusOne;
        public int lastIndex;
        public int freeIndex;

        public IntPinnedArray buckets;

        public T[]    data;
        public PinnedArray<IntHashMapSlot> slots;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntHashMap(in int capacity = 0) {
            this.lastIndex = 0;
            this.length    = 0;
            this.freeIndex = -1;

            this.capacityMinusOne = HashHelpers.GetCapacity(capacity - 1);
            this.capacity         = this.capacityMinusOne + 1;

            this.buckets = new IntPinnedArray(this.capacity);
            this.slots   = new PinnedArray<IntHashMapSlot>(this.capacity);
            this.data    = new T[this.capacity];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntHashMap(IntHashMap<T> other) {
            this.lastIndex = other.lastIndex;
            this.length    = other.length;
            this.freeIndex = other.freeIndex;

            this.capacityMinusOne = other.capacityMinusOne;
            this.capacity         = other.capacity;

            this.buckets = new IntPinnedArray(this.capacity);
            this.slots   = new PinnedArray<IntHashMapSlot>(this.capacity);
            this.data    = new T[this.capacity];

            for (int i = 0, len = this.capacity; i < len; i++) {
                this.buckets.data[i] = other.buckets.data[i];
                this.slots.data[i] = other.slots.data[i];
                this.data[i] = other.data[i];
            }
        }

        ~IntHashMap() {
            this.buckets.Dispose();
            this.slots.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.hashMap = this;
            e.index   = 0;
            e.current = default;
            return e;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public unsafe struct Enumerator {
            public IntHashMap<T> hashMap;

            public int index;
            public int current;

            public bool MoveNext() {
                for (; this.index < this.hashMap.lastIndex; ++this.index) {
                    ref var slot = ref this.hashMap.slots.ptr[this.index];
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
        }
    }
}