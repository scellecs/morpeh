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
        public static bool HasComponent<TNative>(this NativeCache<TNative> nativeCache, in EntityId entityId) where TNative : unmanaged, IComponent {
            return nativeCache.world.Has(in entityId) && nativeCache.components.TryGetIndex(in entityId.internalId) != -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative GetComponent<TNative>(this NativeCache<TNative> nativeCache, in EntityId entityId) where TNative : unmanaged, IComponent {
            return ref nativeCache.components.GetValueRefByKey(in entityId.internalId);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative GetComponent<TNative>(this NativeCache<TNative> nativeCache, in EntityId entityId, out bool exists) where TNative : unmanaged, IComponent {
            exists = nativeCache.world.Has(in entityId) && nativeCache.HasComponent(in entityId);
            return ref nativeCache.components.GetValueRefByKey(in entityId.internalId);
        }
    }
}
#endif