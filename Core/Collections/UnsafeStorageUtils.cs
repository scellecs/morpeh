namespace Morpeh.Collections {
    using System;
    using System.Runtime.InteropServices;
    public static class UnsafeStorageUtils {
        internal static unsafe void AllocateUnsafeArray<T>(UnsafeStorage<T>* unsafeArray, int capacity) where T : unmanaged {
            var align = Marshal.SizeOf<T>();
            var size = capacity * align;
            
            unsafeArray->Ptr = Marshal.AllocHGlobal(size);
            unsafeArray->Capacity = capacity;
            unsafeArray->Length = 0;
            unsafeArray->IsCreated = true;
        }
        
        internal static unsafe void DeallocateUnsafeArray<T>(UnsafeStorage<T>* unsafeArray) where T : unmanaged {
            if (unsafeArray == null || !unsafeArray->IsCreated) {
                return;
            }

            unsafeArray->IsCreated = false;
            unsafeArray->Capacity = 0;
            unsafeArray->Length = 0;
            Marshal.FreeHGlobal(unsafeArray->Ptr);
        }
        
        internal static unsafe void ResizeUnsafeArray<T>(UnsafeStorage<T>* unsafeArray, int capacity) where T : unmanaged {
            var align = Marshal.SizeOf<T>();
            var size = capacity * align;
            var newPtr = Marshal.AllocHGlobal(size);
            
            if (capacity > unsafeArray->Capacity) {
                var oldSize = unsafeArray->Capacity * align;
                Buffer.MemoryCopy(unsafeArray->Ptr.ToPointer(), newPtr.ToPointer(), size, oldSize);
            }
            else {
                Buffer.MemoryCopy(unsafeArray->Ptr.ToPointer(), newPtr.ToPointer(), size, size);
            }
            
            Marshal.FreeHGlobal(unsafeArray->Ptr);
            unsafeArray->Ptr = newPtr;
            unsafeArray->Capacity = capacity;
            unsafeArray->Length = Math.Min(unsafeArray->Length, capacity);
        }
    }
}