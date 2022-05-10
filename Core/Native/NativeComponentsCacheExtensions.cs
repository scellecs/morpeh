#if MORPEH_BURST
namespace Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Morpeh;

    public static class NativeComponentsCacheExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeCache<TNative> AsNative<TNative>(this ComponentsCache<TNative> cache) where TNative : unmanaged, IComponent {
            var nativeCache = new NativeCache<TNative> {
                components = cache.components.AsNative(),
            };
            return nativeCache;
        }
        
                
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponent<TNative>(this NativeCache<TNative> nativeCache, int entityId) where TNative : unmanaged, IComponent {
            return entityId != -1 && nativeCache.components.TryGetIndex(entityId) != -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative GetComponent<TNative>(this NativeCache<TNative> nativeCache, int entityId, out bool exists) where TNative : unmanaged, IComponent {
            exists = nativeCache.HasComponent(entityId);
            return ref nativeCache.components.GetValueRefByKey(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative GetComponent<TNative>(this NativeCache<TNative> nativeCache, int entityId) where TNative : unmanaged, IComponent {
            return ref nativeCache.components.GetValueRefByKey(entityId);
        }
    }
}
#endif