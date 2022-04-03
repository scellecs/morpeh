#if MORPEH_BURST
namespace Morpeh.NativeCollections {
    using System.Runtime.CompilerServices;
    using NativeCollections;
    using Unity.Collections;

    public static class FilterExtensionsForNative {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFilter AsNative(this Filter filter) => new NativeFilter(filter.AsNativeWrapper(), filter.Length);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static NativeFilterWrapper AsNativeWrapper(this Filter filter) {
            // TODO: Get rid of archetypes NativeArray allocation (?)
            var nativeFilter = new NativeFilterWrapper {
                archetypes = new NativeArray<NativeArchetypeWrapper>(filter.archetypes.length, Allocator.TempJob),
            };

            for (int i = 0, length = filter.archetypes.length; i < length; i++) {
                nativeFilter.archetypes[i] = filter.archetypes.data[i].AsNative();
            }

            return nativeFilter;
        }
    }
}
#endif