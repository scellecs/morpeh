#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace Scellecs.Morpeh.WorldBrowser.Utils {
    internal unsafe static class RewindableAllocatorExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* Allocate<T>(this ref RewindableAllocator allocator, int itemsCount) where T : unmanaged {
            return (T*)allocator.Allocate(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), itemsCount);
        }
    }
}
#endif
