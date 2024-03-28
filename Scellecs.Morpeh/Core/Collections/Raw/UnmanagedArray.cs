namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public unsafe struct UnmanagedArray<T> : IEnumerable<T>, IDisposable where T : unmanaged {
        private UnmanagedStorage<T>* ptr;
        private bool IsUnsafeArrayAllocated => this.ptr != null;
        
        public int  Length    => this.IsCreated ? this.ptr->Length : -1;
        public bool IsCreated => this.IsUnsafeArrayAllocated && this.ptr->IsCreated;

        public static UnmanagedArray<T> Create(int length) {
            var array = new UnmanagedArray<T>();
            
            array.ptr = (UnmanagedStorage<T>*) UnmanagedUtils.Malloc<T>(1);
            UnmanagedStorageUtils.AllocateUnsafeArray<T>(array.ptr, length);
            array.ptr->Length = length;

            return array;
        }

        public T this[int index] {
            get {
                if (!this.IsCreated) {
                    throw new Exception("UnmanagedArray is not created");
                }
                if (index < 0 || index >= this.Length) {
                    throw new IndexOutOfRangeException();
                }
                return ((T*) this.ptr->Ptr.ToPointer())[index];
            }
            
            set {
                if (!this.IsCreated) {
                    throw new Exception("UnmanagedArray is not created");
                }
                if (index < 0 || index >= this.Length) {
                    throw new IndexOutOfRangeException();
                }
                ((T*) this.ptr->Ptr.ToPointer())[index] = value;
            }
        }
        
        public ref T GetElementAsRef(int index) {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedArray is not created");
            }
            if (index < 0 || index >= this.Length) {
                throw new IndexOutOfRangeException();
            }
            return ref ((T*) this.ptr->Ptr.ToPointer())[index];
        }

        public void Resize(int length) {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedArray is not created");
            }
            
            if (length == this.Length) {
                return;
            }
            
            UnmanagedStorageUtils.ResizeUnsafeArray<T>(this.ptr, length);
            this.ptr->Length = length;
        }

        public void Dispose() {
            if (!this.IsUnsafeArrayAllocated) {
                return;
            }
            
            UnmanagedStorageUtils.DeallocateUnsafeArray<T>(this.ptr);
            
            UnmanagedUtils.Free<T>((IntPtr) this.ptr);
            this.ptr = null;
        }


        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedArray is not created");
            }

            Enumerator e;
            e.array    = this;
            e.current = -1;
            e.length  = this.Length;

            return e;
        }

        public struct Enumerator : IEnumerator<T> {
            public UnmanagedArray<T> array;

            public int current;
            public int length;
            
            public bool MoveNext() => ++this.current < this.length;
            public void Reset()    => this.current = -1;

            public T Current => ((T*)this.array.ptr->Ptr.ToPointer())[this.current];

            object IEnumerator.Current => this.Current;

            public void Dispose() {
            }
        }
    }
}