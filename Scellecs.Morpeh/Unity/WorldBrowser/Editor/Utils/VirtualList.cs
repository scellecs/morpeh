#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System;

namespace Scellecs.Morpeh.WorldBrowser.Editor.Utils {
    internal sealed class VirtualList<T> : IList<T>, IList {
        private IList<T> innerList;

        internal VirtualList(IList<T> innerList) {
            this.innerList = innerList ?? throw new ArgumentNullException(nameof(innerList));
        }

        internal void SetList(IList<T> newList) {
            this.innerList = newList;
        }

        public T this[int index] {
            get => this.innerList[index];
            set => this.innerList[index] = value;
        }

        object IList.this[int index] {
            get => this[index];
            set {
                if (value is T typedValue) {
                    this[index] = typedValue;
                }
                else {
                    throw new ArgumentException($"Value must be of type {typeof(T).Name}");
                }
            }
        }

        public int Count => this.innerList.Count;
        public bool IsReadOnly => this.innerList.IsReadOnly;

        public void Add(T item) { this.innerList.Add(item); }
        public void Clear() { this.innerList.Clear(); }
        public bool Contains(T item) { return this.innerList.Contains(item); }
        public void CopyTo(T[] array, int arrayIndex) { this.innerList.CopyTo(array, arrayIndex); }
        public IEnumerator<T> GetEnumerator() { return this.innerList.GetEnumerator(); }
        public int IndexOf(T item) { return this.innerList.IndexOf(item); }
        public void Insert(int index, T item) { this.innerList.Insert(index, item); }
        public bool Remove(T item) { return this.innerList.Remove(item); }
        public void RemoveAt(int index) { this.innerList.RemoveAt(index); }
        IEnumerator IEnumerable.GetEnumerator() { return this.innerList.GetEnumerator(); }

        bool IList.IsFixedSize { get { return false; } }
        bool IList.IsReadOnly { get { return this.IsReadOnly; } }

        int IList.Add(object value) {
            if (value is T typedValue) {
                this.Add(typedValue);
                return this.Count - 1;
            }
            else {
                throw new ArgumentException($"Value must be of type {typeof(T).Name}");
            }
        }

        bool IList.Contains(object value) { 
            return value is T typedValue && this.Contains(typedValue); 
        }

        int IList.IndexOf(object value) {
            return value is T typedValue ? this.IndexOf(typedValue) : -1;
        }

        void IList.Insert(int index, object value) {
            if (value is T typedValue) {
                this.Insert(index, typedValue);
            }
            else {
                throw new ArgumentException($"Value must be of type {typeof(T).Name}");
            }
        }

        void IList.Remove(object value) {
            if (value is T typedValue) {
                this.Remove(typedValue);
            }
        }

        void ICollection.CopyTo(Array array, int index) {
            if (array == null) {
                throw new ArgumentNullException(nameof(array));
            }

            if (array.Rank != 1) {
                throw new ArgumentException("Multidimensional arrays are not supported");
            }

            if (array.GetLowerBound(0) != 0) {
                throw new ArgumentException("Array must have zero-based indexing");
            }

            if (index < 0) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (array.Length - index < this.Count) {
                throw new ArgumentException("Destination array is not long enough");
            }

            if (array is T[] typedArray) {
                this.CopyTo(typedArray, index);
                return;
            }

            for (var i = 0; i < this.Count; i++) {
                array.SetValue(this[i], index + i);
            }
        }

        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;
    }
}
#endif