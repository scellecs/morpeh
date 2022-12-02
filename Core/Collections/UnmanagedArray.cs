namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public unsafe struct UnmanagedArray<T> : IEnumerable<T>, IDisposable where T : unmanaged {
        private UnmanagedStorage<T>* ptr;
        private bool IsUnsafeArrayAllocated => this.ptr != null;
        
        public int  Length    => this.IsCreated ? this.ptr->Length : -1;
        public bool IsCreated => this.IsUnsafeArrayAllocated && this.ptr->IsCreated;

        public static UnmanagedArray<T> Create(int length) {
            var array = new UnmanagedArray<T>();
            
            array.ptr = (UnmanagedStorage<T>*) UnsafeUtils.Malloc<T>(1);
            UnmanagedStorageUtils.AllocateUnsafeArray(array.ptr, length);
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
            
            UnmanagedStorageUtils.ResizeUnsafeArray(this.ptr, length);
            this.ptr->Length = length;
        }

        public void Dispose() {
            if (!this.IsUnsafeArrayAllocated) {
                return;
            }
            
            UnmanagedStorageUtils.DeallocateUnsafeArray(this.ptr);
            
            UnsafeUtils.Free<T>((IntPtr) this.ptr);
            this.ptr = null;
        }

        public IEnumerator<T> GetEnumerator() {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedArray is not created");
            }
            
            for (int i = 0, length = this.Length; i < length; i++) {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
    }
}