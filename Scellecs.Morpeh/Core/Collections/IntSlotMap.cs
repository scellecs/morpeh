namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using JetBrains.Annotations;
    using Scellecs.Morpeh.Collections;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal unsafe sealed class IntSlotMap : IDisposable {
        internal const int PAGE_SHIFT = 8;
        internal const int PAGE_SIZE  = 1 << PAGE_SHIFT;
        internal const int PAGE_MASK  = PAGE_SIZE - 1;

        internal int                       length;
        internal int                       capacity;
        internal int                       lastIndex;
        internal int                       freeIndex;
        internal int**                     pages;
        internal int                       pagesLength;
        internal IntHashMapSlotPinnedArray slots;

        public IntSlotMap(int capacity) {
            this.lastIndex = 0;
            this.length = 0;
            this.freeIndex = -1;

            this.capacity = HashHelpers.GetCapacity(capacity - 1) + 1;

            this.pagesLength = 1;
            this.pages = (int**)Marshal.AllocHGlobal(sizeof(int*));
            this.pages[0] = null;

            this.slots = new IntHashMapSlotPinnedArray(this.capacity);
        }
        
        public void Dispose() {
            this.lastIndex = 0;
            this.length = 0;
            this.freeIndex = -1;
            this.capacity = 0;

            if (this.pages != null) {
                for (var i = 0; i < this.pagesLength; i++) {
                    if (this.pages[i] != null) {
                        Marshal.FreeHGlobal((IntPtr)this.pages[i]);
                    }
                }

                Marshal.FreeHGlobal((IntPtr)this.pages);
                this.pages = null;
                this.pagesLength = 0;
            }

            this.slots.Dispose();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetKeyBySlotIndex(int slotIndex) {
            return this.slots.ptr[slotIndex].key - 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(int key) {
            var pageIndex = (uint)key >> PAGE_SHIFT;
            if (pageIndex >= (uint)this.pagesLength) {
                return false;
            }

            var page = this.pages[pageIndex];
            return page != null && page[key & PAGE_MASK] != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int key, out int slotIndex) {
            var pageIndex = (uint)key >> PAGE_SHIFT;
            if (pageIndex < (uint)this.pagesLength && this.pages[pageIndex] != null) {
                ref var entry = ref this.pages[pageIndex][key & PAGE_MASK];
                if (entry != 0) {
                    slotIndex = entry - 1;
                    entry = 0;

                    ref var slot = ref this.slots.ptr[slotIndex];
                    slot.key = -1;
                    slot.next = this.freeIndex;

                    --this.length;
                    if (this.length == 0) {
                        this.lastIndex = 0;
                        this.freeIndex = -1;
                    }
                    else {
                        this.freeIndex = slotIndex;
                    }

                    return true;
                }
            }

            slotIndex = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetIndex(int key, out int slotIndex) {
            var pageIndex = (uint)key >> PAGE_SHIFT;
            if (pageIndex < (uint)this.pagesLength) {
                var page = this.pages[pageIndex];
                if (page != null) {
                    slotIndex = page[key & PAGE_MASK] - 1;
                    if (slotIndex >= 0) {
                        return true;
                    }
                }
            }

            slotIndex = default;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsKeySet(int key, out int slotIndex) {
            var pageIndex = (uint)key >> PAGE_SHIFT;
            if (pageIndex < (uint)this.pagesLength) {
                var page = this.pages[pageIndex];
                if (page != null) {
                    slotIndex = page[key & PAGE_MASK] - 1;
                    return slotIndex >= 0;
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

            var pageIndex = (uint)key >> PAGE_SHIFT;
            if (pageIndex >= (uint)this.pagesLength) {
                this.GrowPages(pageIndex);
            }

            var page = this.pages[pageIndex];
            if (page == null) {
                page = this.pages[pageIndex] = AllocPage();
            }

            page[key & PAGE_MASK] = slotIndex + 1;

            ref var newSlot = ref this.slots.ptr[slotIndex];
            newSlot.key = key + 1;
            newSlot.next = -1;

            ++this.length;
            
            return slotIndex;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private int Expand() {
            var newCapacity = HashHelpers.GetCapacity(this.length) + 1;

            this.slots.Resize(newCapacity);
            this.capacity = newCapacity;

            return newCapacity;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowPages(uint pageIndex) {
            var newLength = this.pagesLength > 0 ? this.pagesLength : 1;
            while ((uint)newLength <= pageIndex) {
                newLength <<= 1;
            }

            var newPages = (int**)Marshal.AllocHGlobal(newLength * sizeof(int*));
            for (var i = 0; i < this.pagesLength; i++) {
                newPages[i] = this.pages[i];
            }
            for (var i = this.pagesLength; i < newLength; i++) {
                newPages[i] = null;
            }

            if (this.pages != null) {
                Marshal.FreeHGlobal((IntPtr)this.pages);
            }
            this.pages = newPages;
            this.pagesLength = newLength;
        }

        private static int* AllocPage() {
            var page = (int*)Marshal.AllocHGlobal(PAGE_SIZE * sizeof(int));
            new Span<int>(page, PAGE_SIZE).Clear();
            return page;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            for (var i = 0; i < this.lastIndex; i++) {
                var key = this.slots.ptr[i].key - 1;
                if (key >= 0) {
                    this.pages[(uint)key >> PAGE_SHIFT][key & PAGE_MASK] = 0;
                }
            }

            this.slots.Clear();
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