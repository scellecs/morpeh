#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Collections;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class NativeFilterExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFilter AsNative(this Filter filter, int length = -1) {
            filter.chunks.Clear();
            for (int i = 0, len = filter.archetypes.length; i < len; i++) {
                filter.chunks.Add(filter.archetypes.data[i].AsChunk());
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