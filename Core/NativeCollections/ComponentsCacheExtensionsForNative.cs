#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    using System.Runtime.CompilerServices;
    using Morpeh;

    public static class ComponentsCacheExtensionsForNative {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeCache<TNative> AsNative<TNative>(this ComponentsCache<TNative> cache) where TNative : unmanaged, IComponent {
            var nativeCache = new NativeCache<TNative> {
                components = cache.components.AsNative(),
            };
            return nativeCache;
        }
    }
}
#endif