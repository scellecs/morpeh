#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static unsafe class NativeStashExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static NativeIntHashMap<TNative> AsNativeIntHashMap<TNative>(this Stash<TNative> hashMap) where TNative : unmanaged, IComponent {
            var nativeIntHashMap = new NativeIntHashMap<TNative>();
            
            fixed (int* lengthPtr = &hashMap.map.length)
            fixed (int* capacityPtr = &hashMap.map.capacity)
            fixed (int* capacityMinusOnePtr = &hashMap.map.capacityMinusOne)
            fixed (int* lastIndexPtr = &hashMap.map.lastIndex)
            fixed (int* freeIndexPtr = &hashMap.map.freeIndex)
            fixed (TNative* dataPtr = &hashMap.data[0]) {
                nativeIntHashMap.lengthPtr           = lengthPtr;
                nativeIntHashMap.capacityPtr         = capacityPtr;
                nativeIntHashMap.capacityMinusOnePtr = capacityMinusOnePtr;
                nativeIntHashMap.lastIndexPtr        = lastIndexPtr;
                nativeIntHashMap.freeIndexPtr        = freeIndexPtr;
                nativeIntHashMap.data                = dataPtr;
                nativeIntHashMap.buckets             = hashMap.map.buckets.ptr;
                nativeIntHashMap.slots               = hashMap.map.slots.ptr;
            }

            return nativeIntHashMap;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeStash<TNative> AsNative<TNative>(this Stash<TNative> stash) where TNative : unmanaged, IComponent {
            var nativeCache = new NativeStash<TNative> {
                components = stash.AsNativeIntHashMap(),
                world = stash.world.AsNative(),
            };
            return nativeCache;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<TNative>(this NativeStash<TNative> nativeStash, in EntityId entityId) where TNative : unmanaged, IComponent {
            return nativeStash.world.Has(in entityId) && nativeStash.components.TryGetIndex(in entityId.id) != -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative Get<TNative>(this ref NativeStash<TNative> nativeStash, in EntityId entityId) where TNative : unmanaged, IComponent {
            return ref nativeStash.components.GetValueRefByKey(in entityId.id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative Get<TNative>(this ref NativeStash<TNative> nativeStash, in EntityId entityId, out bool exists) where TNative : unmanaged, IComponent {
            exists = nativeStash.world.Has(in entityId) && nativeStash.Has(in entityId);
            return ref nativeStash.components.GetValueRefByKey(in entityId.id);
        }
    }
}
#endif