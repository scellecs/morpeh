namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using Scellecs.Morpeh.Collections;
    
    public static class WorldEntityExtensions {
        [PublicAPI]
        public static Entity CreateEntity(this World world) {
            world.ThreadSafetyCheck();
            
            int id;
            if (world.freeEntityIDs.length > 0) {
                id = world.freeEntityIDs.Pop();
            }
            else {
                id = ++world.entitiesLength;
            }

            if (world.entitiesLength >= world.entitiesCapacity) {
                world.ExpandEntities();
            }

            ++world.entitiesCount;
            return world.GetEntityAtIndex(id);
        }

        [PublicAPI]
        public static Entity CreateEntity(this World world, out int id) {
            world.ThreadSafetyCheck();

            if (world.freeEntityIDs.length > 0) {
                id = world.freeEntityIDs.Pop();
            }
            else {
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
            
            Array.Resize(ref world.entities, newCapacity);
            for (var i = oldCapacity; i < newCapacity; i++)
            {
                world.entities[i].Initialize();
            }
            
            Array.Resize(ref world.entitiesGens, newCapacity);
            
            world.dirtyEntities.Resize(newCapacity);
            world.disposedEntities.Resize(newCapacity);

            world.entitiesCapacity = newCapacity;
        }

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            
            if (world.dirtyEntities.Contains(entity.Id)) {
                var changesCount = entityData.changesCount;
                
                for (var i = 0; i < changesCount; i++) {
                    var structuralChange = entityData.changes[i];

                    if (!structuralChange.isAddition) {
                        continue;
                    }
                    
                    world.GetStash(structuralChange.typeOffset.GetValue())?.Clean(entity);
                }
                
                world.dirtyEntities.Remove(entity.Id);
            }
            
            // Clear components from existing archetype
            
            if (entityData.currentArchetype != null) {
                foreach (var offset in entityData.currentArchetype.components) {
                    world.GetStash(offset)?.Clean(entity);
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
                   entity.WorldId != world.identifier;
        }

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(this World world, Entity entity) {
            world.ThreadSafetyCheck();
            
            return entity.Id > 0 &&
                   entity.Id < world.entitiesCapacity &&
                   world.entitiesGens[entity.Id] == entity.Generation &&
                   entity.WorldId == world.identifier;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Entity GetEntityAtIndex(this World world, int entityId) {
            return new Entity(world.identifier, entityId, world.entitiesGens[entityId]);
        }
    }
}