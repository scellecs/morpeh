namespace SourceGenerators.Utils.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>> where T : IEquatable<T> {
        private readonly T[]? array;
        
        public EquatableArray(T[] array) {
            this.array = array;
        }
        
        public EquatableArray(List<T>? list) {
            if (list == null || list.Count == 0) {
                this.array = null;
                return;
            }
            
            this.array = list.ToArray();
        }
        
        public int Length => this.array?.Length ?? 0;
        
        public T this[int index] {
            get {
                if (this.array == null) {
                    throw new IndexOutOfRangeException();
                }
                
                return this.array[index];
            }
            
            set {
                if (this.array == null) {
                    throw new IndexOutOfRangeException();
                }
                
                this.array[index] = value;
            }
        }
        
        public ReadOnlySpan<T> AsSpan() => this.array.AsSpan();
        
        public bool Equals(EquatableArray<T> otherArray) => this.AsSpan().SequenceEqual(otherArray.AsSpan());
        public override bool Equals(object? obj) => obj is EquatableArray<T> otherArray && this.Equals(otherArray);

        public override int GetHashCode() {
            if (this.array == null) {
                return 0;
            }

            unchecked {
                var hash = this.array.Length * 397;
                
                for (int i = 0, length = this.array.Length; i < length; i++) {
                    hash = hash * 31 + this.array[i].GetHashCode();
                }
                
                return hash;
            }
        }
        
        public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);
        public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);
        
        public Enumerator GetEnumerator() => new(this.array ?? Array.Empty<T>());
        
        public struct Enumerator {
            private readonly T[] array;
            private          int index;
            
            public Enumerator(T[] array) {
                this.array = array;
                this.index = -1;
            }
            
            public bool MoveNext() => ++this.index < this.array.Length;
            public T Current => this.array[this.index];
        }
    }
}