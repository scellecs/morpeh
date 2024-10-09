namespace Scellecs.Morpeh {
    using Unity.IL2CPP.CompilerServices;
    using System.Runtime.CompilerServices;
    using Scellecs.Morpeh.Collections;
    using System;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal struct ComponentsToFiltersRelation {
        internal Filter[][] componentsToFilters;
        
        public ComponentsToFiltersRelation(int capacity) {
            this.componentsToFilters = new Filter[capacity][];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Filter[] GetFilters(int typeId) {
            return typeId >= this.componentsToFilters.Length ? null : this.componentsToFilters[typeId];
        }

        public void Add(int[] typeIds, Filter filter) {
            var maxTypeId = this.GetMaxTypeId(typeIds);
            
            if (maxTypeId >= this.componentsToFilters.Length) {
                var newLength = this.componentsToFilters.Length;
                while (maxTypeId >= newLength) {
                    newLength *= 2;
                }
                
                this.ResizeUpTo(newLength);
            }
            
            foreach (var typeInfo in typeIds) {
                this.AppendFilter(typeInfo, filter);
            }
        }

        public void Remove(int[] typeIds, Filter filter) {
            foreach (var typeId in typeIds) {
                if (typeId < this.componentsToFilters.Length && this.componentsToFilters[typeId] != null) {
                    this.RemoveFilter(typeId, filter);
                }
            }
        }

        private int GetMaxTypeId(int[] typeIds) {
            var maxTypeId = 0;
            foreach (var typeId in typeIds) {
                if (typeId > maxTypeId) {
                    maxTypeId = typeId;
                }
            }
            return maxTypeId;
        }
        
        private void ResizeUpTo(int newLength) {
            var newComponentsToFilters = new Filter[newLength][];
            for (var i = 0; i < this.componentsToFilters.Length; i++) {
                newComponentsToFilters[i] = this.componentsToFilters[i];
            }
            this.componentsToFilters = newComponentsToFilters;
        }

        private void AppendFilter(int typeId, Filter filter) {
            if (this.componentsToFilters[typeId] == null) {
                this.componentsToFilters[typeId] = new Filter[1];
                this.componentsToFilters[typeId][0] = filter;
            } else {
                ref var filters = ref this.componentsToFilters[typeId];
                var position = filters.Length;
                
                ArrayHelpers.Grow(ref filters, filters.Length + 1);
                filters[position] = filter;
            }
        }

        private void RemoveFilter(int typeId, Filter filter) {
            ref var filters = ref this.componentsToFilters[typeId];
            var index = Array.IndexOf(filters, filter);
            if (index != -1) {
                var last = filters.Length - 1;
                if (index < last) {
                    Array.Copy(filters, index + 1, filters, index, last - index);
                }

                Array.Resize(ref filters, filters.Length - 1);
                this.componentsToFilters[typeId] = filters;
            }
        }
    }
}