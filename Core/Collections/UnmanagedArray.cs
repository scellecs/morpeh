using System.Collections;
using System.Collections.Generic;

namespace Morpeh.Collections {
    using System;
    using System.Runtime.InteropServices;
    
    public unsafe struct UnmanagedArray<T> :IEnumerable<T> where T : unmanaged {
        private UnsafeArray<T>* ptr;
        
        private bool IsUnsafeArrayAllocated => this.ptr != null;
        public int Length => this.IsCreated ? ptr->Length : -1;
        public bool IsCreated => IsUnsafeArrayAllocated && ptr->IsCreated;

        public UnmanagedArray(int length) {
            var size = Marshal.SizeOf<UnsafeArray<T>>();
            this.ptr = (UnsafeArray<T>*) Marshal.AllocHGlobal(size).ToPointer();
            AllocateUnsafeArray(this.ptr, length);
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
        
        private void AllocateUnsafeArray(UnsafeArray<T>* unsafeArray, int length) {
            var align = Marshal.SizeOf<T>();
            var size = length * align;
            
            unsafeArray->Ptr = Marshal.AllocHGlobal(size);
            unsafeArray->Length = length;
            unsafeArray->IsCreated = true;
        }
        
        private void DeallocateUnsafeArray(UnsafeArray<T>* unsafeArray) {
            if (unsafeArray == null || !unsafeArray->IsCreated) {
                return;
            }

            unsafeArray->IsCreated = false;
            unsafeArray->Length = 0;
            Marshal.FreeHGlobal(unsafeArray->Ptr);
        }
        
        public void Resize(int length) {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedArray is not created");
            }
            
            if (length == this.Length) {
                return;
            }
            
            ResizeInternal(this.ptr, length);
        }

        private void ResizeInternal(UnsafeArray<T>* unsafeArray, int length) {
            var align = Marshal.SizeOf<T>();
            var size = length * align;
            var newPtr = Marshal.AllocHGlobal(size);
            
            if (length > unsafeArray->Length) {
                var oldSize = unsafeArray->Length * align;
                Buffer.MemoryCopy(unsafeArray->Ptr.ToPointer(), newPtr.ToPointer(), size, oldSize);
            }
            else {
                Buffer.MemoryCopy(unsafeArray->Ptr.ToPointer(), newPtr.ToPointer(), size, size);
            }
            
            Marshal.FreeHGlobal(unsafeArray->Ptr);
            unsafeArray->Ptr = newPtr;
            unsafeArray->Length = length;
        }

        public void Dispose() {
            if (!this.IsUnsafeArrayAllocated) {
                return;
            }
            
            DeallocateUnsafeArray(this.ptr);
            
            Marshal.FreeHGlobal((IntPtr) this.ptr);
            this.ptr = null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (!this.IsCreated) {
                throw new Exception("UnmanagedArray is not created");
            }
            
            for (int i = 0, length = this.Length; i < length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}