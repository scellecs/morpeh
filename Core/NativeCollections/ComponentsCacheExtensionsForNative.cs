#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    using System.Runtime.CompilerServices;
    using Morpeh;

    internal static class ComponentsCacheExtensionsForNative {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static NativeCacheWrapper<TNative> AsNative<TNative>(this ComponentsCache<TNative> cache) where TNative : unmanaged, IComponent {
            var nativeCache = new NativeCacheWrapper<TNative> {
                components = cache.components.AsNative(),
            };
            return nativeCache;
        }
    }
}
#endif