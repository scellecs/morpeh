#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif
#if !MORPEH_DEBUG
#define MORPEH_DEBUG_DISABLED
#endif

namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Collections;
    using JetBrains.Annotations;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class EntityExtensions {
#if !MORPEH_STRICT_MODE
        [Obsolete("This method is slow and doesn't have a Stash-based alternative. Consider doing manual migration of required components.")]
        public static void MigrateTo(this Entity from, Entity to, bool overwrite = true) {
#if MORPEH_DEBUG
            if (from.IsNullOrDisposed()) {
                InvalidMigrateOperationException.ThrowDisposedEntityFrom(from);
            }
            
            if (to.IsNullOrDisposed()) {
                InvalidMigrateOperationException.ThrowDisposedEntityTo(to);
            }
#endif

            var world = from.GetWorld();
            ref var fromEntityData = ref world.entities[from.Id];
            
            // We have to make a full copy because Migrate would modify the original data
            
            Span<int> addedComponents = stackalloc int[fromEntityData.addedComponentsCount];
            fromEntityData.addedComponents.CopyTo(addedComponents);
            
            Span<int> removedComponents = stackalloc int[fromEntityData.removedComponentsCount];
            fromEntityData.removedComponents.CopyTo(removedComponents);
            
            // Migrate all newly added components from transient archetype
            
            foreach (var typeId in addedComponents) {
                world.GetExistingStash(typeId)?.Migrate(from, to, overwrite);
            }

            if (fromEntityData.currentArchetype == null) {
                return;
            }
            
            // Migrate all components that are not removed from the source entity from current archetype
            
            foreach (var typeId in fromEntityData.currentArchetype.components) {
                var wasRemoved = false;
                
                foreach (var removedTypeId in removedComponents) {
                    if (typeId != removedTypeId) {
                        continue;
                    }
                    
                    wasRemoved = true;
                    break;
                }
                
                if (wasRemoved) {
                    continue;
                }
                
                world.GetExistingStash(typeId)?.Migrate(from, to, overwrite);
            }
        }
        
#endif

        [Obsolete("[MORPEH] Use World.RemoveEntity() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dispose(this Entity entity) {
            entity.GetWorld()?.RemoveEntity(entity);
        }
        
        [Obsolete("[MORPEH] Use World.IsDisposed() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDisposed(this Entity entity) {
            if (entity == default) {
                return true;
            }
            
            var world = entity.GetWorld();
            return world == null || world.IsDisposed(entity);
        }

        [Obsolete("[MORPEH] Use World.IsDisposed() instead. This is the same as IsDisposed() but with a different name for compatibility.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrDisposed(this Entity entity) {
            return entity.IsDisposed();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static World GetWorld(this Entity entity) {
            var worldId = entity.WorldId;

            if (worldId < 0 || worldId >= WorldConstants.MAX_WORLDS_COUNT) {
                return null;
            }

            var index = World.worldsIndices[worldId];

            if (index == 0) {
                return null;
            }

            var world = World.worlds[index - 1];

            if (entity.WorldGeneration != World.worldsGens[worldId]) {
                return null;
            }

            return world;
        }
    }
}
