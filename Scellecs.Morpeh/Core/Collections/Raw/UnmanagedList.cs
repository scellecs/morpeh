namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public unsafe struct UnmanagedList<T> : IEnumerable<T>, IDisposable where T : unmanaged {
        private UnmanagedStorage<T>* ptr;
        private bool                 IsUnsafeArrayAllocated => this.ptr != null;

        public int  Length    => this.IsCreated ? this.ptr->Length : -1;
        public int  Capacity  => this.IsCreated ? this.ptr->Capacity : -1;
        public bool IsCreated => this.IsUnsafeArrayAllocated && this.ptr->IsCreated;

        public static UnmanagedList<T> Create(int capacity = 2) {
            var list = new UnmanagedList<T>();

            list.ptr = (UnmanagedStorage<T>*)UnmanagedUtils.Malloc<T>(1);
            UnmanagedStorageUtils.AllocateUnsafeArray<T>(list.ptr, capacity);
            list.ptr->Length = 0;

            return list;
        }

        public T this[int index] {
            get {
                if (!this.IsCreated) {
                    throw new Exception("UnmanagedList is not created");
                }
                if (index < 0 || index >= this.Length) {
                    throw new IndexOutOfRangeException();
                }
                return ((T*)this.ptr->Ptr.ToPointer())[index];
            }

            set {
                if (!this.IsCreated) {
                    throw new Exception("UnmanagedList is not created");
                }
                if (index < 0 || index >= this.Length) {
                    throw new IndexOutOfRangeException();
                }
                ((T*)this.ptr->Ptr.ToPointer())[index] = value;
            }
        }

        public ref T GetElementAsRef(int index) {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedList is not created");
            }
            if (index < 0 || index >= this.Length) {
                throw new IndexOutOfRangeException();
            }
            return ref ((T*)this.ptr->Ptr.ToPointer())[index];
        }

        public void Add(T element) {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedList is not created");
            }

            if (this.Length == this.Capacity) {
                UnmanagedStorageUtils.ResizeUnsafeArray<T>(this.ptr, this.Capacity * 2);
            }

            var length = this.Length;

            this.ptr->Length++;
            this[length] = element;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index) {
            --this.ptr->Length;
            if (index < this.ptr->Length) {
                UnmanagedUtils.MemCpy<T>((IntPtr)this.ptr+index, (IntPtr)this.ptr+index+1, this.ptr->Length - index);
            }

            *(T*)this.ptr->Ptr.ToPointer() = default;
        }

        public void RemoveAtSwapBack(int index) {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedList is not created");
            }
            if (index < 0 || index >= this.Length) {
                throw new IndexOutOfRangeException();
            }
            this[index]           = this[this.Length - 1];
            this[this.Length - 1] = default;
            this.ptr->Length--;
        }

        public void Clear() {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedList is not created");
            }
            this.ptr->Length = 0;
        }

        public void Dispose() {
            if (!this.IsUnsafeArrayAllocated) {
                return;
            }

            UnmanagedStorageUtils.DeallocateUnsafeArray<T>(this.ptr);
            UnmanagedUtils.Free<T>((IntPtr)this.ptr);
            this.ptr = null;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedList is not created");
            }

            Enumerator e;
            e.list    = this;
            e.current = -1;
            e.length  = this.Length;

            return e;
        }

        public struct Enumerator : IEnumerator<T> {
            public UnmanagedList<T> list;

            public int  current;
            public int  length;
            
            public bool MoveNext() => ++this.current < this.length;
            public void Reset()    => this.current = -1;

            public T Current => ((T*)this.list.ptr->Ptr.ToPointer())[this.current];

            object IEnumerator.Current => this.Current;

            public void Dispose() {
            }
        }
    }
}