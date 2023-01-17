namespace Scellecs.Morpeh.Collections {
    using System;
    public static class UnmanagedStorageUtils {
        internal static unsafe void AllocateUnsafeArray<T>(UnmanagedStorage<T>* unsafeArray, int capacity) where T : unmanaged {
            unsafeArray->Ptr       = UnmanagedUtils.Malloc<T>(capacity);
            unsafeArray->Capacity  = capacity;
            unsafeArray->Length    = 0;
            unsafeArray->IsCreated = true;
        }
        
        internal static unsafe void DeallocateUnsafeArray<T>(UnmanagedStorage<T>* unsafeArray) where T : unmanaged {
            if (unsafeArray == null || !unsafeArray->IsCreated) {
                return;
            }

            unsafeArray->IsCreated = false;
            UnmanagedUtils.Free<T>(unsafeArray->Ptr);
            unsafeArray->Capacity = 0;
            unsafeArray->Length = 0;
        }
        
        internal static unsafe void ResizeUnsafeArray<T>(UnmanagedStorage<T>* unsafeArray, int capacity) where T : unmanaged {
            var newPtr = UnmanagedUtils.Malloc<T>(capacity);
            
            if (capacity > unsafeArray->Capacity) {
                var oldSize = unsafeArray->Capacity;
                UnmanagedUtils.MemCpy<T>(newPtr, unsafeArray->Ptr, oldSize);
            }
            else {
                UnmanagedUtils.MemCpy<T>(newPtr, unsafeArray->Ptr, capacity);
            }
            
            UnmanagedUtils.Free<T>(unsafeArray->Ptr);
            unsafeArray->Ptr = newPtr;
            unsafeArray->Capacity = capacity;
            unsafeArray->Length = Math.Min(unsafeArray->Length, capacity);
        }
    }
}