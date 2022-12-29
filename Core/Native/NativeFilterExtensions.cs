#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Unity.Collections;

    public static class NativeFilterExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFilter AsNative(this Filter filter, int length = -1) {
            CreateNativeFilter(filter, out var nativeFilter);
            nativeFilter.length = length == -1 ? filter.GetLengthSlow() : length;
            return nativeFilter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void CreateNativeFilter(Filter filter, out NativeFilter nativeFilter) {
            // TODO: Get rid of archetypes NativeArray allocation (?)
            nativeFilter = new NativeFilter {
                archetypes = new NativeArray<NativeArchetype>(filter.archetypes.length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory),
                world = filter.world.AsNative(),
            };

            for (int i = 0, length = filter.archetypes.length; i < length; i++) {
                nativeFilter.archetypes[i] = filter.archetypes.data[i].AsNative();
            }
        }
    }
}
#endif