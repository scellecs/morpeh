#if MORPEH_BURST
namespace Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Morpeh;

    public static class NativeCacheExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeCache<TNative> AsNative<TNative>(this ComponentsCache<TNative> cache) where TNative : unmanaged, IComponent {
            var nativeCache = new NativeCache<TNative> {
                components = cache.components.AsNative(),
                world = cache.world.AsNative(),
            };
            return nativeCache;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasComponent<TNative>(this NativeCache<TNative> nativeCache, EntityId entityId) where TNative : unmanaged, IComponent {
            return nativeCache.world.Has(in entityId) && nativeCache.components.TryGetIndex(entityId.internalId) != -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative GetComponent<TNative>(this NativeCache<TNative> nativeCache, EntityId entityId) where TNative : unmanaged, IComponent {
            return ref nativeCache.components.GetValueRefByKey(entityId.internalId);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative GetComponent<TNative>(this NativeCache<TNative> nativeCache, EntityId entityId, out bool exists) where TNative : unmanaged, IComponent {
            exists = nativeCache.world.Has(in entityId) && nativeCache.HasComponent(entityId);
            return ref nativeCache.components.GetValueRefByKey(entityId.internalId);
        }
    }
}
#endif