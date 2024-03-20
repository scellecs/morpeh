namespace Scellecs.Morpeh {
    using System;
    
    // TODO: Rework to struct
    internal class ComponentsToFiltersRelation {
        internal Filter[][] componentsToFilters;
        
        public ComponentsToFiltersRelation(int capacity = 32) {
            this.componentsToFilters = new Filter[capacity][];
        }
        
        public Filter[] GetFilters(int offset) {
            return offset > this.componentsToFilters.Length ? null : this.componentsToFilters[offset];
        }

        public void Add(TypeInfo[] typeInfos, Filter filter) {
            var maxTypeOffset = this.GetMaxOffset(typeInfos);
            
            if (maxTypeOffset.GetValue() >= this.componentsToFilters.Length) {
                var newLength = this.componentsToFilters.Length;
                while (maxTypeOffset.GetValue() >= newLength) {
                    newLength *= 2;
                }
                
                this.ResizeUpTo(newLength);
            }
            
            foreach (var typeOffset in typeInfos) {
                this.AppendFilter(typeOffset, filter);
            }
        }
        
        private TypeOffset GetMaxOffset(TypeInfo[] typeInfos) {
            var maxTypeOffset = new TypeOffset(-1);
            foreach (var typeInfo in typeInfos) {
                if (typeInfo.offset > maxTypeOffset) {
                    maxTypeOffset = typeInfo.offset;
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

        private void AppendFilter(TypeInfo typeInfo, Filter filter) {
            var offsetValue = typeInfo.offset.GetValue();
            
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