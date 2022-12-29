namespace Scellecs.Morpeh.Collections {
    public static class UnmanagedUtils {
        public static int SizeOf<T>() where T : unmanaged {
#if MORPEH_BURST
            return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<T>();
#else
            return System.Runtime.InteropServices.Marshal.SizeOf<T>();
#endif
        }

        public static unsafe System.IntPtr Malloc<T>(int length) where T : unmanaged {
#if MORPEH_BURST
            var alignment = Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AlignOf<T>();
            return (System.IntPtr) Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Malloc(length * SizeOf<T>(), alignment, Unity.Collections.Allocator.Persistent);
#else
            return System.Runtime.InteropServices.Marshal.AllocHGlobal(length * SizeOf<T>());
#endif
        }

        public static unsafe void Free<T>(System.IntPtr ptr) where T : unmanaged {
#if MORPEH_BURST
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.Free((void*) ptr, Unity.Collections.Allocator.Persistent);
#else
            System.Runtime.InteropServices.Marshal.FreeHGlobal(ptr);
#endif
        }
        
        public static unsafe void MemCpy<T>(System.IntPtr dst, System.IntPtr src, int length) where T : unmanaged {
#if MORPEH_BURST
            Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy((void*) dst, (void*) src, length * SizeOf<T>());
#else
            System.Buffer.MemoryCopy((void*) src, (void*) dst, length * SizeOf<T>(), length * SizeOf<T>());
#endif
        }
    }
}