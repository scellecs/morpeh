namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Scellecs.Morpeh.Collections;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class LongSlotMap {
        internal int               length;
        internal int               capacity;
        internal int               lastIndex;
        internal int               freeIndex;
        internal long              capacityMinusOne;
        internal int[]             buckets;
        internal LongHashMapSlot[] slots;

        public LongSlotMap(int capacity) {
            this.lastIndex = 0;
            this.length = 0;
            this.freeIndex = -1;

            this.capacity = HashHelpers.GetCapacity(capacity - 1) + 1;
            this.capacityMinusOne = this.capacity - 1;

            this.buckets = new int[this.capacity];
            this.slots = new LongHashMapSlot[this.capacity];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetKeyBySlotIndex(int slotIndex) {
            return this.slots[slotIndex].key - 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(long key) {
            var rem = (int)(key & this.capacityMinusOne);

            int next;
            for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                var slot = this.slots[i];
                if (slot.key - 1 == key) {
                    return true;
                }

                next = slot.next;
            }

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(long key, out int slotIndex) {
            var rem = (int)(key & this.capacityMinusOne);

            int next;
            int num = -1;
            for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref this.slots[i];
                if (slot.key - 1 == key) {
                    if (num < 0) {
                        this.buckets[rem] = slot.next + 1;
                    }
                    else {
                        this.slots[num].next = slot.next;
                    }

                    slotIndex = i;

                    slot.key = -1;
                    slot.next = this.freeIndex;

                    --this.length;
                    if (this.length == 0) {
                        this.lastIndex = 0;
                        this.freeIndex = -1;
                    }
                    else {
                        this.freeIndex = i;
                    }

                    return true;
                }

                next = slot.next;
                num  = i;
            }

            slotIndex = default;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetIndex(long key, out int slotIndex) {
            var rem = (int)(key & this.capacityMinusOne);

            int next;
            for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                var slot = this.slots[i];
                if (slot.key - 1 == key) {
                    slotIndex = i;
                    return true;
                }

                next = slot.next;
            }

            slotIndex = default;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeySet(long key, out int slotIndex) {
            var rem = (int)(key & this.capacityMinusOne);

            for (var i = this.buckets[rem] - 1; i >= 0; i = this.slots[i].next) {
                if (this.slots[i].key - 1 == key) {
                    slotIndex = i;
                    return true;
                }
            }
            
            slotIndex = -1;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() {
            return this.length == 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCapacityFull() {
            return this.lastIndex == this.capacity;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TakeSlot(long key, out bool resized) {
            resized = false;
            
            int slotIndex;
            if (this.freeIndex >= 0) {
                slotIndex = this.freeIndex;
                this.freeIndex = this.slots[slotIndex].next;
            }
            else {
                if (this.IsCapacityFull()) {
                    this.Expand();
                    resized = true;
                }

                slotIndex = this.lastIndex;
                ++this.lastIndex;
            }
            
            var rem = (int)(key & this.capacityMinusOne);
            ref var newSlot = ref this.slots[slotIndex];

            newSlot.key = key + 1;
            newSlot.next = this.buckets[rem] - 1;

            this.buckets[rem] = slotIndex + 1;
            ++this.length;
            
            return slotIndex;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private int Expand() {
            var newCapacityMinusOne = HashHelpers.GetCapacity(this.length);
            var newCapacity= newCapacityMinusOne + 1;

            ArrayHelpers.Grow(ref this.slots, newCapacity);

            var newBuckets = new int[newCapacity];

            for (int i = 0, len = this.lastIndex; i < len; ++i) {
                ref var slot = ref this.slots[i];

                var newResizeIndex = (slot.key - 1) & newCapacityMinusOne;
                slot.next = newBuckets[newResizeIndex] - 1;

                newBuckets[newResizeIndex] = i + 1;
            }

            this.buckets = newBuckets;
            this.capacity = newCapacity;
            this.capacityMinusOne = newCapacityMinusOne;
            
            return newCapacity;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            Array.Clear(this.slots, 0, this.lastIndex);
            Array.Clear(this.buckets, 0, this.capacity);
            this.lastIndex = 0;
            this.length = 0;
            this.freeIndex = -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.map = this;
            e.index = 0;
            e.current = default;
            return e;
        }
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator {
            public LongSlotMap map;

            public int index;
            public int current;

            public bool MoveNext() {
                for (; this.index < this.map.lastIndex; ++this.index) {
                    var slot = this.map.slots[this.index];
                    if (slot.key - 1 < 0) {
                        continue;
                    }

                    this.current = this.index;
                    ++this.index;

                    return true;
                }

                this.index = this.map.lastIndex + 1;
                this.current = default;
                return false;
            }

            public int Current => this.current;
        }
    }
}