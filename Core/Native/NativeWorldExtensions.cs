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
            fixed (int* entitiesCountPtr = &world.entitiesCount)
            {
                nativeWorld.entitiesGens = entitiesGensPtr;
                nativeWorld.entitiesCount = entitiesCountPtr;
            }
            
            return nativeWorld;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool Has(this NativeWorld nativeWorld, in EntityId entityId)
        {
            if (entityId.internalId < 0 || entityId.internalId >= *nativeWorld.entitiesCount)
                return false;
            
            return nativeWorld.entitiesGens[entityId.internalId] == entityId.internalGen;
        }
    }
}