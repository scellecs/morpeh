namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    using Scellecs.Morpeh.Collections;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class Archetype : IDisposable {
        internal ArchetypeHash hash;
        
        internal EntityPinnedArray entities;
        internal int length;
        internal int capacity;
        
        internal IntHashSet components;

        internal IntSlotMap filtersMap;
        internal Filter[]   filters;
       
        internal Archetype(ArchetypeHash hash) {
            this.hash = hash;
            
            this.entities = new EntityPinnedArray(16);
            this.length   = 0;
            this.capacity = this.entities.Length;
            
            this.components = new IntHashSet(15);
            
            this.filtersMap = new IntSlotMap(3);
            this.filters    = new Filter[this.filtersMap.capacity];
        }
        
        public void Dispose() {
            this.hash = default;
            
            this.entities.Dispose();
            this.entities = default;
            this.length = 0;
            this.capacity = 0;
            
            this.components = null;
            
            this.filtersMap = null;
            this.filters = null;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int AddEntity(Entity entity) {
            if (this.length == this.capacity) {
                this.GrowEntities();
            }
            
            this.entities[this.length] = entity;
            return this.length++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntityAtIndex(int index) {
            this.entities[index] = this.entities[--this.length];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty() {
            return this.length == 0;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void GrowEntities() {
            this.capacity <<= 1;
            this.entities.Resize(this.capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddFilter(Filter filter) {
            var slotIndex = this.filtersMap.TakeSlot(filter.id, out var resized);
            if (resized) {
                this.GrowFilters();
            }
            
            this.filters[slotIndex] = filter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveFilter(Filter filter) { 
            if (this.filtersMap.Remove(filter.id, out var slotIndex)) {
                this.filters[slotIndex] = default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearFilters() {
            Array.Clear(this.filters, 0, this.filtersMap.lastIndex);
            this.filtersMap.Clear();
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void GrowFilters() {
            ArrayHelpers.Grow(ref this.filters, this.filtersMap.capacity);
        }
    }
}
