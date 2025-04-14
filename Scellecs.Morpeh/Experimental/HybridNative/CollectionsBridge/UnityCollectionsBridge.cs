using System.Runtime.CompilerServices;

namespace Unity.Collections {
    public static unsafe class UnityCollectionsBridge {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Allocate(long size, int align, Unity.Collections.AllocatorManager.AllocatorHandle allocator) { 
            return Memory.Unmanaged.Allocate(size, align, allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free(void* pointer, Unity.Collections.AllocatorManager.AllocatorHandle allocator) { 
            Memory.Unmanaged.Free(pointer, allocator);
        }

        public static class AllocatorManager {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Allocator LegacyOf(Unity.Collections.AllocatorManager.AllocatorHandle handle) { 
                return Unity.Collections.AllocatorManager.LegacyOf(handle);
            }
        }

        public static class CollectionHelper {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static unsafe void* AlignPointer(void* ptr, int alignmentPowerOfTwo) { 
                return Unity.Collections.CollectionHelper.AlignPointer(ptr, alignmentPowerOfTwo);
            }
        }

        public static class ConcurrentMask {
            public const int ErrorFailedToFree = Unity.Collections.ConcurrentMask.ErrorFailedToFree;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int TryFree(ref long l, int offset, int bits) {
                return Unity.Collections.ConcurrentMask.TryFree(ref l, offset, bits);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int TryFree<T>(ref T t, int offset, int bits) where T : IIndexable<long> {
                return Unity.Collections.ConcurrentMask.TryFree(ref t, offset, bits);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long MakeMask(int offset, int bits) { 
                return Unity.Collections.ConcurrentMask.MakeMask(offset, bits);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long AtomicOr(ref long destination, long source) { 
                return Unity.Collections.ConcurrentMask.AtomicOr(ref destination, source);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static long AtomicAnd(ref long destination, long source) {
                return Unity.Collections.ConcurrentMask.AtomicAnd(ref destination, source);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool foundAtLeastThisManyConsecutiveZeroes(long value, int minimum, out int offset, out int count) { 
                return Unity.Collections.ConcurrentMask.foundAtLeastThisManyConsecutiveZeroes(value, minimum, out offset, out count);
            }
        }
    }
}
