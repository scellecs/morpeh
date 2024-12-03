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
        public static NativeFilter AsNative(this Filter filter) {
            if (filter.chunks == null) {
                filter.chunks = new FastList<Filter.Chunk>(filter.archetypes.Length);
            } else {
                filter.chunks.Clear();
                
                if (filter.chunks.capacity < filter.archetypes.Length) {
                    filter.chunks.Grow(filter.archetypes.Length);
                }
            }
            
            var entitiesLength = 0;
            
            for (int i = 0, len = filter.archetypesLength; i < len; i++) {
                entitiesLength += filter.archetypes[i].length;
                filter.chunks.Add(filter.archetypes[i].AsChunk(filter.world));
            }
            
            var nativeFilter = new NativeFilter {
                archetypes = filter.chunks.AsNative(),
                world = filter.world.AsNative(),
                length = entitiesLength,
            };

            return nativeFilter;
        }
    }
}
#endif