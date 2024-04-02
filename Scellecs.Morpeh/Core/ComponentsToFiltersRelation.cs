namespace Scellecs.Morpeh {
    using System;
    using Unity.IL2CPP.CompilerServices;
    using System.Runtime.CompilerServices;
    using Scellecs.Morpeh.Collections;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal struct ComponentsToFiltersRelation {
        internal Filter[][] componentsToFilters;
        
        public ComponentsToFiltersRelation(int capacity) {
            this.componentsToFilters = new Filter[capacity][];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Filter[] GetFilters(int offset) {
            return offset >= this.componentsToFilters.Length ? null : this.componentsToFilters[offset];
        }

        public void Add(TypeOffset[] typeOffsets, Filter filter) {
            var maxTypeOffset = this.GetMaxOffset(typeOffsets);
            
            if (maxTypeOffset.GetValue() >= this.componentsToFilters.Length) {
                var newLength = this.componentsToFilters.Length;
                while (maxTypeOffset.GetValue() >= newLength) {
                    newLength *= 2;
                }
                
                this.ResizeUpTo(newLength);
            }
            
            foreach (var typeInfo in typeOffsets) {
                this.AppendFilter(typeInfo, filter);
            }
        }
        
        private TypeOffset GetMaxOffset(TypeOffset[] typeOffsets) {
            var maxTypeOffset = new TypeOffset(-1);
            foreach (var typeOffset in typeOffsets) {
                if (typeOffset.GetValue() > maxTypeOffset.GetValue()) {
                    maxTypeOffset = typeOffset;
                }
            }
            return maxTypeOffset;
        }
        
        private void ResizeUpTo(int newLength) {
            var newComponentsToFilters = new Filter[newLength][];
            for (var i = 0; i < this.componentsToFilters.Length; i++) {
                newComponentsToFilters[i] = this.componentsToFilters[i];
            }
            this.componentsToFilters = newComponentsToFilters;
        }

        private void AppendFilter(TypeOffset typeInfo, Filter filter) {
            var offsetValue = typeInfo.GetValue();
            
            if (this.componentsToFilters[offsetValue] == null) {
                this.componentsToFilters[offsetValue] = new Filter[1];
                this.componentsToFilters[offsetValue][0] = filter;
            }
            else {
                ref var filters = ref this.componentsToFilters[offsetValue];
                var position = filters.Length;
                
                Array.Resize(ref filters, filters.Length + 1);
                filters[position] = filter;
            }
        }
    }
}