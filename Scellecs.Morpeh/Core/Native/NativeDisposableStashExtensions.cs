﻿#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Unity.Burst.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static unsafe class NativeDisposableStashExtensions {        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeDisposableStash<TNative> AsNative<TNative>(this DisposableStash<TNative> stash) where TNative : unmanaged, IDisposableComponent {
            var slotMap = stash.map;
            var nativeIntSlotMap = new NativeIntSlotMap();
            var nativeStash = default(NativeDisposableStash<TNative>);

            fixed (int* capacityMinusOnePtr = &slotMap.capacityMinusOne)
            fixed (TNative* dataPtr = &stash.data[0]) 
            fixed (TNative* emptyPtr = &stash.empty) {
                nativeIntSlotMap.capacityMinusOnePtr = capacityMinusOnePtr;
                nativeIntSlotMap.buckets = slotMap.buckets.ptr;
                nativeIntSlotMap.slots = slotMap.slots.ptr;
                nativeStash.data = dataPtr;
                nativeStash.empty = emptyPtr;
            }

            nativeStash.map = nativeIntSlotMap;
            nativeStash.world = stash.world.AsNative();

            return nativeStash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<TNative>(this NativeDisposableStash<TNative> nativeStash, Entity entity) where TNative : unmanaged, IDisposableComponent {
            return nativeStash.world.Has(in entity) && nativeStash.map.IndexOf(entity.Id) != -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative Get<TNative>(this ref NativeDisposableStash<TNative> nativeStash, Entity entity) where TNative : unmanaged, IDisposableComponent {
            var idx = nativeStash.map.IndexOf(entity.Id);
            if (Hint.Likely(idx >= 0)) {
                return ref nativeStash.data[idx];
            }

            return ref *nativeStash.empty;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TNative Get<TNative>(this ref NativeDisposableStash<TNative> nativeStash, Entity entity, out bool exists) where TNative : unmanaged, IDisposableComponent {
            var idx = nativeStash.map.IndexOf(entity.Id);
            exists = idx >= 0 && nativeStash.world.Has(entity);
            if (exists) {
                return ref nativeStash.data[idx];
            }

            return ref *nativeStash.empty;
        }
    }
}
#endif