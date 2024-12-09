namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Collections;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal sealed class SwappableLongSlotMap {
        internal int               length;
        internal int               capacity;
        internal long              capacityMinusOne;
        internal int[]             buckets;
        internal LongHashMapSlot[] slots;

        public SwappableLongSlotMap(int capacity) {
            this.length = 0;

            this.capacity         = HashHelpers.GetCapacity(capacity - 1) + 1;
            this.capacityMinusOne = this.capacity - 1;

            this.buckets = new int[this.capacity];
            this.slots   = new LongHashMapSlot[this.capacity];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetKeyBySlotIndex(int slotIndex) => this.slots[slotIndex].key - 1;

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
        public bool Remove(long key, out int slotIndex, out int swappedFromSlotIndex) {
            swappedFromSlotIndex = -1;

            if (!this.TryGetIndex(key, out slotIndex)) {
                return false;
            }

            ref var removedSlot = ref this.slots[slotIndex];

            var     rem    = (int)(key & this.capacityMinusOne);
            ref var bucket = ref this.buckets[rem];

            if (bucket - 1 == slotIndex) {
                bucket = removedSlot.next + 1;
            }
            else {
                var prevIndex = bucket - 1;
                while (prevIndex >= 0) {
                    ref var prevSlot = ref this.slots[prevIndex];
                    if (prevSlot.next == slotIndex) {
                        prevSlot.next = removedSlot.next;
                        break;
                    }

                    prevIndex = prevSlot.next;
                }
            }

            var lastSlotIndex = --this.length;

            if (slotIndex == lastSlotIndex) {
                removedSlot.key = -1;
                return true;
            }

            ref var lastSlot = ref this.slots[lastSlotIndex];

            var     lastSlotKey    = lastSlot.key;
            var     lastSlotRem    = (int)((lastSlotKey - 1) & this.capacityMinusOne);
            ref var lastSlotBucket = ref this.buckets[lastSlotRem];

            if (lastSlotBucket - 1 == lastSlotIndex) {
                lastSlotBucket = slotIndex + 1;
            }
            else {
                var prevIndex = lastSlotBucket - 1;
                while (prevIndex >= 0) {
                    ref var prevSlot = ref this.slots[prevIndex];
                    if (prevSlot.next == lastSlotIndex) {
                        prevSlot.next = slotIndex;
                        break;
                    }

                    prevIndex = prevSlot.next;
                }
            }

            removedSlot.key      = lastSlotKey;
            removedSlot.next     = lastSlot.next;
            swappedFromSlotIndex = lastSlotIndex;

            lastSlot.key = -1;

            return true;
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
        public bool IsEmpty() => this.length == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TakeSlot(long key, out bool resized) {
            resized = false;

            if (this.length == this.capacity) {
                this.Expand();
                resized = true;
            }

            var     slotIndex = this.length++;
            var     rem       = (int)(key & this.capacityMinusOne);
            ref var newSlot   = ref this.slots[slotIndex];

            newSlot.key  = key + 1;
            newSlot.next = this.buckets[rem] - 1;

            this.buckets[rem] = slotIndex + 1;

            return slotIndex;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private int Expand() {
            var newCapacityMinusOne = HashHelpers.GetCapacity(this.length);
            var newCapacity         = newCapacityMinusOne + 1;
            
            ArrayHelpers.Grow(ref this.slots, newCapacity);

            var newBuckets = new int[newCapacity];

            for (var i = 0; i < this.length; ++i) {
                ref var slot = ref this.slots[i];

                var newResizeIndex = (slot.key - 1) & newCapacityMinusOne;
                slot.next = newBuckets[newResizeIndex] - 1;

                newBuckets[newResizeIndex] = i + 1;
            }

            this.buckets          = newBuckets;
            this.capacity         = newCapacity;
            this.capacityMinusOne = newCapacityMinusOne;

            return newCapacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            Array.Clear(this.slots, 0, this.length);
            Array.Clear(this.buckets, 0, this.capacity);
            this.length = 0;
        }
    }
}