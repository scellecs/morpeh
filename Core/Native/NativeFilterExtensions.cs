#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Collections;

    public static class NativeFilterExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFilter AsNative(this Filter filter, int length = -1) {
            filter.chunks.length = 0;
            for (int i = 0, len = filter.archetypes.length; i < len; i++) {
                filter.chunks.Add(filter.archetypes.data[i].AsChunk());
                filter.chunks.length++;
            }
            
            var nativeFilter = new NativeFilter {
                chunks = filter.chunks.AsNative(),
                world = filter.world.AsNative(),
                length = length == -1 ? filter.GetLengthSlow() : length
            };

            return nativeFilter;
        }
    }
}
#endif