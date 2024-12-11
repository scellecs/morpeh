#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class NativeWorldExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NativeWorld AsNative(this World world) {
            var nativeWorld = new NativeWorld();
            fixed (ushort* entitiesGensPtr = world.entitiesGens)
            fixed (int* entitiesCapacityPtr = &world.entitiesCapacity) {
                nativeWorld.identifier = world.identifier;
                nativeWorld.generation = world.generation;
                nativeWorld.entitiesGens = entitiesGensPtr;
                nativeWorld.entitiesCapacity = entitiesCapacityPtr;
            }
            
            return nativeWorld;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool Has(this NativeWorld nativeWorld, in Entity entity) {
            if (entity.Id < 0 || entity.Id >= *nativeWorld.entitiesCapacity) {
                return false;
            }
            
            return entity.Generation == nativeWorld.entitiesGens[entity.Id];
        }
    }
}
#endif