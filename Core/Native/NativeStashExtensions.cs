#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class NativeStashExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeStash<TNative> AsNative<TNative>(this Stash<TNative> stash) where TNative : unmanaged, IComponent {
            var nativeCache = new NativeStash<TNative> {
                components = stash.components.AsNative(),
                world = stash.world.AsNative(),
            };
            return nativeCache;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<TNative>(this NativeStash<TNative> nativeStash, in EntityId entityId) where TNative : unmanaged, IComponent {
            return nativeStash.world.Has(in entityId) && nativeStash.components.TryGetIndex(in entityId.id) != -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative Get<TNative>(this NativeStash<TNative> nativeStash, in EntityId entityId) where TNative : unmanaged, IComponent {
            return ref nativeStash.components.GetValueRefByKey(in entityId.id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative Get<TNative>(this NativeStash<TNative> nativeStash, in EntityId entityId, out bool exists) where TNative : unmanaged, IComponent {
            exists = nativeStash.world.Has(in entityId) && nativeStash.Has(in entityId);
            return ref nativeStash.components.GetValueRefByKey(in entityId.id);
        }
    }
}
#endif