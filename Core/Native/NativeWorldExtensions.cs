#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;

    public static class NativeWorldExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NativeWorld AsNative(this World world) {
            var nativeWorld = new NativeWorld();
            fixed (int* entitiesGensPtr = world.entitiesGens)
            fixed (int* entitiesCapacityPtr = &world.entitiesCapacity) {
                nativeWorld.entitiesGens = entitiesGensPtr;
                nativeWorld.entitiesCapacity = entitiesCapacityPtr;
            }
            
            return nativeWorld;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool Has(this NativeWorld nativeWorld, in EntityId entityId) {
            if (entityId.id < 0 || entityId.id >= *nativeWorld.entitiesCapacity) {
                return false;
            }
            
            return entityId.gen == nativeWorld.entitiesGens[entityId.id];
        }
    }
}
#endif