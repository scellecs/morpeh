namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class IntSparseSet {
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
            
            Array.Resize(ref this.sparse, this.capacity);
            Array.Resize(ref this.dense, this.capacity);
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
            return value < capacity && this.sparse[value] < this.count && this.dense[this.sparse[value]] == value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            for (var i = 0; i < this.count; i++) {
                this.sparse[this.dense[i]] = 0;
            }
            
            this.count = 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            var e = default(Enumerator);
            e.dense = this.dense;
            e.index = -1;
            e.count = this.count;
            return e;
        }
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator {
            internal int[] dense;
            internal int index;
            internal int count;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() {
                return ++this.index < this.count;
            }
            
            public int Current {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.dense[this.index];
            }
        }
    }
}