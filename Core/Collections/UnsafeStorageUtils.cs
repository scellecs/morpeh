namespace Morpeh.Collections {
    using System.Runtime.InteropServices;
    public static class UnsafeStorageUtils {
        internal static unsafe void AllocateUnsafeArray<T>(UnsafeStorage<T>* unsafeArray, int length) where T : unmanaged {
            var align = Marshal.SizeOf<T>();
            var size = length * align;
            
            unsafeArray->Ptr = Marshal.AllocHGlobal(size);
            unsafeArray->Length = length;
            unsafeArray->Capacity = length;
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
    }
}