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

    // TODO: Restore MigrateTo functionality
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class EntityExtensions {
        internal static Entity Create(int id, World world) {
            var newEntity = new Entity { 
                entityId = new EntityId(id, world.entitiesGens[id]), 
                world = world,
            };

            return newEntity;
        }
        
#if !MORPEH_STRICT_MODE
#if MORPEH_LEGACY
        [Obsolete("[MORPEH] Use Stash.Add() instead.")]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this Entity entity) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception("[MORPEH] You are trying AddComponent on null or disposed entity");
            }
#endif
            return ref entity.world.GetStash<T>().Add(entity);
        }

#if MORPEH_LEGACY
        [Obsolete("[MORPEH] Use Stash.Add() instead.")]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this Entity entity, out bool exist) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception("[MORPEH] You are trying AddComponent on null or disposed entity");
            }
#endif
            return ref entity.world.GetStash<T>().Add(entity, out exist);
        }

#if MORPEH_LEGACY
        [Obsolete("[MORPEH] Use Stash.Get() instead.")]
#endif
        public static ref T GetComponent<T>(this Entity entity) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception("[MORPEH] You are trying GetComponent on null or disposed entity");
            }
#endif
            return ref entity.world.GetStash<T>().Get(entity);
        }

#if MORPEH_LEGACY
        [Obsolete("[MORPEH] Use Stash.Get() instead.")]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetComponent<T>(this Entity entity, out bool exist) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception("[MORPEH] You are trying GetComponent on null or disposed entity");
            }
#endif
            return ref entity.world.GetStash<T>().Get(entity, out exist);
        }

#if MORPEH_LEGACY
        [Obsolete("[MORPEH] Use Stash.Set() instead.")]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetComponent<T>(this Entity entity, in T value) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception("[MORPEH] You are trying SetComponent on null or disposed entity");
            }
#endif
            entity.world.GetStash<T>().Set(entity, value);
        }

#if MORPEH_LEGACY
        [Obsolete("[MORPEH] Use Stash.Remove() instead.")]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveComponent<T>(this Entity entity) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception("[MORPEH] You are trying RemoveComponent on null or disposed entity");
            }
#endif
            return entity.world.GetStash<T>().Remove(entity);
        }

#if MORPEH_LEGACY
        [Obsolete("[MORPEH] Use Stash.Migrate() instead.")]
#endif
        public static void Migrate<T>(this Entity from, Entity to, bool overwrite = true) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (from.IsNullOrDisposed() || to.IsNullOrDisposed()) {
                throw new Exception("[MORPEH] You are trying MigrateTo on null or disposed entities");
            }
#endif
            from.world.GetStash<T>().Migrate(from, to, overwrite);
        }

#if MORPEH_LEGACY
        [Obsolete("[MORPEH] Use Stash.Has() instead.")]
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(this Entity entity) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception("[MORPEH] You are trying Has on null or disposed entity");
            }
#endif
            return entity.world.GetStash<T>().Has(entity);
        }
#endif

        // TODO: Move the logic to WorldExtensions and just call it from here
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dispose(this Entity entity) {
            if (entity.IsDisposed()) {
#if MORPEH_DEBUG
                MLogger.LogError($"You're trying to dispose disposed entity with ID {entity.entityId}.");
#endif
                return;
            }
            
            entity.world.ThreadSafetyCheck();
            
            var world = entity.world;
            
            // Clean new components if entity is transient
            
            if (world.dirtyEntities.Get(entity.ID.id)) {
                ref var transient = ref world.transients[entity.ID.id];
                
                // As we clean stashes, changes count may increase, so we need to store it
                var changesCount = transient.changesCount;
                
                for (var i = 0; i < changesCount; i++) {
                    ref var structuralChange = ref transient.changes[i];

                    if (!structuralChange.isAddition) {
                        continue;
                    }
                    
                    world.GetStash(structuralChange.typeOffset.GetValue())?.Clean(entity);
                }
            }
            
            // Clear components from existing archetype
            
            if (entity.currentArchetype != ArchetypeId.Invalid) {
                if (world.archetypes.TryGetValue(entity.currentArchetype.GetValue(), out var archetype)) {
                    foreach (var offset in archetype.components) {
                        world.GetStash(offset)?.Clean(entity);
                    }
                    
                    archetype.Remove(entity.ID);
                    world.TryScheduleArchetypeForRemoval(archetype);
                }
            }
            
            entity.world.ApplyRemoveEntity(entity.entityId.id);
            entity.world.dirtyEntities.Unset(entity.entityId.id);

            entity.DisposeFast();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void DisposeFast(this Entity entity) {
            entity.currentArchetype = ArchetypeId.Invalid;

            entity.world      = null;
            entity.entityId   = EntityId.Invalid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDisposed([NotNull] this Entity entity) => entity.entityId == EntityId.Invalid || entity.world == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrDisposed([CanBeNull] this Entity entity) {
            return entity == null || entity.IsDisposed();
        }
    }
}
