using System.Runtime.CompilerServices;

namespace Morpeh.Native
{
    public static class NativeWorldExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NativeWorld AsNative(this World world)
        {
            var nativeWorld = new NativeWorld();
            fixed (int* entitiesGensPtr = world.entitiesGens)
            fixed (int* entitiesCapacityPtr = &world.entitiesCapacity)
            {
                nativeWorld.entitiesGens = entitiesGensPtr;
                nativeWorld.entitiesCapacity = entitiesCapacityPtr;
            }
            
            return nativeWorld;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool Has(this NativeWorld nativeWorld, in EntityId entityId)
        {
            if (entityId.internalId < 0 || entityId.internalId >= *nativeWorld.entitiesCount)
                return false;
            
            return entityId.internalGen == nativeWorld.entitiesGens[entityId.internalId];
        }
    }
}