namespace Scellecs.Morpeh.Collections {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class IntSparseSet {
        internal int[] sparse;
        internal int[] dense;

        internal int capacity;
        internal int count;
        
        public IntSparseSet(int defaultCapacity) {
            this.capacity = defaultCapacity;
            
            this.sparse = new int[this.capacity];
            this.dense = new int[this.capacity];
            
            this.count = 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newCapacity) {
            this.capacity = newCapacity;
            
            ArrayHelpers.Grow(ref this.sparse, this.capacity);
            ArrayHelpers.Grow(ref this.dense, this.capacity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(int value) {
            if (this.Contains(value)) {
                return false;
            }

            this.dense[this.count] = value;
            this.sparse[value] = this.count;
            
            ++this.count;
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Remove(int value) {
            if (!this.Contains(value)) {
                return false;
            }
            
            var index = this.sparse[value];
            
            --this.count;
            this.dense[index] = this.dense[this.count];
            this.sparse[this.dense[index]] = index;
            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int value) {
            return value < this.capacity && this.sparse[value] < this.count && this.dense[this.sparse[value]] == value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            for (var i = this.count - 1; i >= 0; i--) {
                this.sparse[this.dense[i]] = 0;
            }
            
            this.count = 0;
        }
    }
}