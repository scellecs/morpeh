using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Scellecs.Morpeh.Collections;
using Unity.IL2CPP.CompilerServices;

namespace Scellecs.Morpeh
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public unsafe class StashMap
    {
        internal int length;
        internal int capacity;
        internal int capacityMinusOne;
        internal int lastIndex;
        internal int freeIndex;
        internal IntPinnedArray buckets;
        internal PinnedArray<IntHashMapSlot> slots;

        public StashMap(int capacity) {
            this.lastIndex = 0;
            this.length = 0;
            this.freeIndex = -1;

            this.capacityMinusOne = HashHelpers.GetCapacity(capacity - 1);
            this.capacity = this.capacityMinusOne + 1;

            this.buckets = new IntPinnedArray(this.capacity);
            this.slots = new PinnedArray<IntHashMapSlot>(this.capacity);
        }

        public StashMap(StashMap other) {
            this.lastIndex = other.lastIndex;
            this.length = other.length;
            this.freeIndex = other.freeIndex;

            this.capacityMinusOne = other.capacityMinusOne;
            this.capacity = other.capacity;

            this.buckets = new IntPinnedArray(this.capacity);
            this.slots = new PinnedArray<IntHashMapSlot>(this.capacity);
            
            for (int i = 0, len = this.capacity; i < len; i++) {
                this.buckets.data[i] = other.buckets.data[i];
                this.slots.data[i] = other.slots.data[i];
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKeyByIndex(in int index) {
            return slots.ptr[index].key - 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(in int key) {
            var rem = key & capacityMinusOne;

            int next;
            for (var i = buckets.ptr[rem] - 1; i >= 0; i = next) {
                ref var slot = ref slots.ptr[i];
                if (slot.key - 1 == key) {
                    return true;
                }

                next = slot.next;
            }

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(in int key, [CanBeNull] out int dataIndex) {
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

                    dataIndex = i;

                    slot.key  = -1;
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

            dataIndex = default;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetIndex(in int key, out int dataIndex) {
            var rem = key & this.capacityMinusOne;

            int next;
            for (var i = this.buckets.ptr[rem] - 1; i >= 0; i = next) {
                ref var slot = ref this.slots.ptr[i];
                if (slot.key - 1 == key) {
                    dataIndex = i;
                    return true;
                }

                next = slot.next;
            }

            dataIndex = default;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyFrom(StashMap from, out bool needResize) {
            this.lastIndex = from.lastIndex;
            this.length = from.length;
            this.freeIndex = from.freeIndex;

            needResize = this.capacity < from.capacity;

            this.capacityMinusOne = from.capacityMinusOne;
            this.capacity = from.capacity;

            if (needResize) {
                this.buckets = new IntPinnedArray(this.capacity);
                this.slots = new PinnedArray<IntHashMapSlot>(this.capacity);
            }

            for (int i = 0, len = this.capacity; i < len; i++) {
                this.buckets.data[i] = from.buckets.data[i];
                this.slots.data[i] = from.slots.data[i];
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeySet(in int key, out int slotIndex) {
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
        public int TakeSlot(in int key, out bool resized) {
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

            newSlot.key  = key + 1;
            newSlot.next = this.buckets.ptr[rem] - 1;

            this.buckets.ptr[rem] = slotIndex + 1;
            ++this.length;
            
            return slotIndex;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public int Expand() {
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
    }
}