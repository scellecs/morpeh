namespace Morpeh.Collections {
    using System;
    using System.Runtime.InteropServices;
    public static class UnsafeStorageUtils {
        internal static unsafe void AllocateUnsafeArray<T>(UnsafeStorage<T>* unsafeArray, int capacity) where T : unmanaged {
            unsafeArray->Ptr = UnsafeUtils.Malloc<T>(capacity);
            unsafeArray->Capacity = capacity;
            unsafeArray->Length = 0;
            unsafeArray->IsCreated = true;
        }
        
        internal static unsafe void DeallocateUnsafeArray<T>(UnsafeStorage<T>* unsafeArray) where T : unmanaged {
            if (unsafeArray == null || !unsafeArray->IsCreated) {
                return;
            }

            unsafeArray->IsCreated = false;
            UnsafeUtils.Free<T>(unsafeArray->Ptr);
            unsafeArray->Capacity = 0;
            unsafeArray->Length = 0;
        }
        
        internal static unsafe void ResizeUnsafeArray<T>(UnsafeStorage<T>* unsafeArray, int capacity) where T : unmanaged {
            var newPtr = UnsafeUtils.Malloc<T>(capacity);
            
            if (capacity > unsafeArray->Capacity) {
                var oldSize = unsafeArray->Capacity;
                UnsafeUtils.MemCpy<T>(newPtr, unsafeArray->Ptr, oldSize);
            }
            else {
                UnsafeUtils.MemCpy<T>(newPtr, unsafeArray->Ptr, capacity);
            }
            
            UnsafeUtils.Free<T>(unsafeArray->Ptr);
            unsafeArray->Ptr = newPtr;
            unsafeArray->Capacity = capacity;
            unsafeArray->Length = Math.Min(unsafeArray->Length, capacity);
        }
    }
}