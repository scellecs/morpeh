namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using Scellecs.Morpeh.Collections;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class WorldEntityExtensions {
        [PublicAPI]
        public static Entity CreateEntity(this World world) {
            world.ThreadSafetyCheck();

            if (!world.freeEntityIDs.TryPop(out var id)) {
                id = ++world.entitiesLength;
            }

            if (world.entitiesLength >= world.entitiesCapacity) {
                world.ExpandEntities();
            }

            ++world.entitiesCount;
            return world.GetEntityAtIndex(id);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ExpandEntities(this World world) {
            var oldCapacity = world.entitiesCapacity;
            var newCapacity = HashHelpers.GetCapacity(world.entitiesCapacity) + 1;
            
            ArrayHelpers.Grow(ref world.entities, newCapacity);
            for (var i = oldCapacity; i < newCapacity; i++)
            {
                world.entities[i].Initialize();
            }
            
            ArrayHelpers.Grow(ref world.entitiesGens, newCapacity);
            
            world.dirtyEntities.Resize(newCapacity);
            world.disposedEntities.Resize(newCapacity);

            world.entitiesCapacity = newCapacity;
        }

        [PublicAPI]
        public static void RemoveEntity(this World world, Entity entity) {
            world.ThreadSafetyCheck();
            
            if (world.IsDisposed(entity)) {
#if MORPEH_DEBUG
                MLogger.LogError($"You're trying to dispose disposed entity {entity}.");
#endif
                return;
            }
            
            ref var entityData = ref world.entities[entity.Id];
            
            // Clear new components if entity is transient
            
            if (world.dirtyEntities.Remove(entity.Id)) {
                var addedComponentsCount = entityData.addedComponentsCount;
                
                for (var i = 0; i < addedComponentsCount; i++) {
                    var typeId = entityData.addedComponents[i];
                    world.GetExistingStash(typeId)?.Clean(entity);
                }
            }
            
            // Clear components from existing archetype
            
            if (entityData.currentArchetype != null) {
                foreach (var typeId in entityData.currentArchetype.components) {
                    world.GetExistingStash(typeId)?.Clean(entity);
                }
            }
            
            world.disposedEntities.Add(entity.Id);
            
            world.IncrementGeneration(entity.Id);
            --world.entitiesCount;
        }

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDisposed(this World world, Entity entity) {
            world.ThreadSafetyCheck();
            
            return entity.Id <= 0 ||
                   entity.Id >= world.entitiesCapacity ||
                   world.entitiesGens[entity.Id] != entity.Generation ||
                   entity.WorldId != world.identifier ||
                   entity.WorldGeneration != world.generation;
        }

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(this World world, Entity entity) {
            world.ThreadSafetyCheck();
            
            return entity.Id > 0 &&
                   entity.Id < world.entitiesCapacity &&
                   world.entitiesGens[entity.Id] == entity.Generation &&
                   entity.WorldId == world.identifier &&
                   entity.WorldGeneration == world.generation;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Entity GetEntityAtIndex(this World world, int entityId) {
            return new Entity(world.identifier, world.generation, entityId, world.entitiesGens[entityId]);
        }
    }
}