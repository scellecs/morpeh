namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct LongHashMapSlot {
        public long key;
        public int next;
    }

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class LongHashMap<T> {
        public int length;
        public int capacity;
        public int capacityMinusOne;
        public int lastIndex;
        public int freeIndex;

        public IntPinnedArray buckets;

        public T[]    data;
        public PinnedArray<LongHashMapSlot> slots;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LongHashMap(in int capacity = 0) {
            this.lastIndex = 0;
            this.length    = 0;
            this.freeIndex = -1;

            this.capacityMinusOne = HashHelpers.GetCapacity(capacity - 1);
            this.capacity         = this.capacityMinusOne + 1;

            this.buckets = new IntPinnedArray(this.capacity);
            this.slots   = new PinnedArray<LongHashMapSlot>(this.capacity);
            this.data    = new T[this.capacity];
        }

        ~LongHashMap() {
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
            public LongHashMap<T> hashMap;

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