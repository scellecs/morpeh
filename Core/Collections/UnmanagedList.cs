namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public unsafe struct UnmanagedList<T> : IEnumerable<T>, IDisposable  where T : unmanaged {
        private UnsafeStorage<T>* ptr;
        private bool IsUnsafeArrayAllocated => this.ptr != null;
        
        public int  Length    => this.IsCreated ? this.ptr->Length : -1;
        public int  Capacity  => this.IsCreated ? this.ptr->Capacity : -1;
        public bool IsCreated => this.IsUnsafeArrayAllocated && this.ptr->IsCreated;

        public static UnmanagedList<T> Create(int capacity = 2)
        {
            var list = new UnmanagedList<T>();
            
            list.ptr = (UnsafeStorage<T>*) UnsafeUtils.Malloc<T>(1);
            UnsafeStorageUtils.AllocateUnsafeArray(list.ptr, capacity);
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
                return ((T*) this.ptr->Ptr.ToPointer())[index];
            }
            
            set {
                if (!this.IsCreated) {
                    throw new Exception("UnmanagedList is not created");
                }
                if (index < 0 || index >= this.Length) {
                    throw new IndexOutOfRangeException();
                }
                ((T*) this.ptr->Ptr.ToPointer())[index] = value;
            }
        }
        
        public ref T GetElementAsRef(int index) {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedList is not created");
            }
            if (index < 0 || index >= this.Length) {
                throw new IndexOutOfRangeException();
            }
            return ref ((T*) this.ptr->Ptr.ToPointer())[index];
        }

        public void Add(T element)
        {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedList is not created");
            }
            
            if (this.Length == this.Capacity) {
                UnsafeStorageUtils.ResizeUnsafeArray(this.ptr, this.Capacity * 2);
            }
            
            var length = this.Length;
            
            this.ptr->Length++;
            this[length] = element;
        }
        
        public void RemoveAtSwapBack(int index) {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedList is not created");
            }
            if (index < 0 || index >= this.Length) {
                throw new IndexOutOfRangeException();
            }
            this[index] = this[this.Length - 1];
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
            
            UnsafeStorageUtils.DeallocateUnsafeArray(this.ptr);
            UnsafeUtils.Free<T>((IntPtr) this.ptr);
            this.ptr = null;
        }

        public IEnumerator<T> GetEnumerator() {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedList is not created");
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