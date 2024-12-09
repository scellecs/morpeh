namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using Scellecs.Morpeh.Collections;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal unsafe sealed class IntSlotMap : IDisposable {
        internal int                       length;
        internal int                       capacity;
        internal int                       capacityMinusOne;
        internal int                       lastIndex;
        internal int                       freeIndex;
        internal IntPinnedArray            buckets;
        internal IntHashMapSlotPinnedArray slots;

        public IntSlotMap(int capacity) {
            this.lastIndex = 0;
            this.length = 0;
            this.freeIndex = -1;

            this.capacityMinusOne = HashHelpers.GetCapacity(capacity - 1);
            this.capacity = this.capacityMinusOne + 1;

            this.buckets = new IntPinnedArray(this.capacity);
            this.slots = new IntHashMapSlotPinnedArray(this.capacity);
        }
        
        public void Dispose() {
            this.lastIndex = 0;
            this.length = 0;
            this.freeIndex = -1;
            this.capacityMinusOne = 0;
            this.capacity = 0;
            this.buckets.Dispose();
            this.slots.Dispose();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKeyBySlotIndex(int slotIndex) {
            return this.slots.ptr[slotIndex].key - 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(int key) {
            var rem = key & this.capacityMinusOne;

            int next;
            for (var i = this.buckets.ptr[rem] - 1; i >= 0; i = next) {
                var slot = this.slots.ptr[i];
                if (slot.key - 1 == key) {
                    return true;
                }

                next = slot.next;
            }

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key, out int slotIndex) {
            var rem = key & this.capacityMinusOne;

            int next;
            int num = -1;
            for (var i = this.buckets.ptr[rem] - 1; i >= 0; i = next) {
                ref var slot = ref this.slots.ptr[i];
                if (slot.key - 1 == key) {
                    if (num < 0) {
                        this.buckets.ptr[rem] = slot.next + 1;
                    }
                    else {
                        this.slots.ptr[num].next = slot.next;
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
        public bool TryGetIndex(int key, out int slotIndex) {
            var rem = key & this.capacityMinusOne;

            int next;
            for (var i = this.buckets.ptr[rem] - 1; i >= 0; i = next) {
                var slot = this.slots.ptr[i];
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
        public bool IsKeySet(int key, out int slotIndex) {
            var rem = key & this.capacityMinusOne;

            for (var i = this.buckets.ptr[rem] - 1; i >= 0; i = this.slots.ptr[i].next) {
                if (this.slots.ptr[i].key - 1 == key) {
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
        public int TakeSlot(int key, out bool resized) {
            resized = false;
            
            int slotIndex;
            if (this.freeIndex >= 0) {
                slotIndex = this.freeIndex;
                this.freeIndex = this.slots.ptr[slotIndex].next;
            }
            else {
                if (this.IsCapacityFull()) {
                    this.Expand();
                    resized = true;
                }

                slotIndex = this.lastIndex;
                ++this.lastIndex;
            }
            
            var rem = key & this.capacityMinusOne;
            ref var newSlot = ref this.slots.ptr[slotIndex];

            newSlot.key = key + 1;
            newSlot.next = this.buckets.ptr[rem] - 1;

            this.buckets.ptr[rem] = slotIndex + 1;
            ++this.length;
            
            return slotIndex;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private int Expand() {
            var newCapacityMinusOne = HashHelpers.GetCapacity(this.length);
            var newCapacity= newCapacityMinusOne + 1;

            this.slots.Resize(newCapacity);

            var newBuckets = new IntPinnedArray(newCapacity);

            for (int i = 0, len = this.lastIndex; i < len; ++i) {
                ref var slot = ref this.slots.ptr[i];

                var newResizeIndex = (slot.key - 1) & newCapacityMinusOne;
                slot.next = newBuckets.ptr[newResizeIndex] - 1;

                newBuckets.ptr[newResizeIndex] = i + 1;
            }

            this.buckets.Dispose();
            this.buckets = newBuckets;
            this.capacity = newCapacity;
            this.capacityMinusOne = newCapacityMinusOne;
            
            return newCapacity;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            this.slots.Clear();
            this.buckets.Clear();
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
            public IntSlotMap map;

            public int index;
            public int current;

            public bool MoveNext() {
                for (; this.index < this.map.lastIndex; ++this.index) {
                    var slot = this.map.slots.ptr[this.index];
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