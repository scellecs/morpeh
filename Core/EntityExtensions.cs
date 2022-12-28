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
        internal static Entity Create(int id, int worldID)
        {
            var world = World.worlds.data[worldID];
            var newEntity = new Entity { entityId = new EntityId(id, world.entitiesGens[id]), worldID = worldID };

            newEntity.world = world;

            newEntity.previousArchetypeId = -1;
            newEntity.currentArchetypeId  = 0;

            newEntity.currentArchetype = newEntity.world.archetypes.data[0];

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
            var cache = entity.world.GetStash<T>();

            return ref cache.Add(entity);
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
            var cache = entity.world.GetStash<T>();

            return ref cache.Add(entity, out exist);
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
            var cache = entity.world.GetStash<T>();

            return ref cache.Get(entity);
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
            var cache = entity.world.GetStash<T>();

            return ref cache.Get(entity, out exist);
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
            var cache = entity.world.GetStash<T>();

            cache.Set(entity, value);
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
            var cache = entity.world.GetStash<T>();

            return cache.Remove(entity);
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

            var cache = entity.world.GetStash<T>();
            return cache.Has(entity);
        }

#if MORPEH_LEGACY
        [Obsolete("[MORPEH] Use Stash.Migrate() instead.")]
#endif
        public static void MigrateTo(this Entity from, Entity to, bool overwrite = true) {
#if MORPEH_DEBUG
            if (from.IsNullOrDisposed() || to.IsNullOrDisposed()) {
                throw new Exception("[MORPEH] You are trying MigrateTo on null or disposed entities");
            }
#endif

            var world = from.world;
            foreach (var cacheId in world.stashes) {
                var cache = Stash.stashes.data[world.stashes.GetValueByIndex(cacheId)];
                cache.Migrate(from, to, overwrite);
            }
        }
        
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddTransfer(this Entity entity, int typeId) {
            if (entity.previousArchetypeId == -1) {
                entity.previousArchetypeId = entity.currentArchetypeId;
            }

            entity.currentArchetype.AddTransfer(typeId, out entity.currentArchetypeId, out entity.currentArchetype);
            if (entity.isDirty == true) {
                return;
            }

            entity.world.dirtyEntities.Set(entity.entityId.id);
            entity.isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveTransfer(this Entity entity, int typeId) {
            if (entity.previousArchetypeId == -1) {
                entity.previousArchetypeId = entity.currentArchetypeId;
            }

            entity.currentArchetype.RemoveTransfer(typeId, out entity.currentArchetypeId, out entity.currentArchetype);
            if (entity.isDirty == true) {
                return;
            }

            entity.world.dirtyEntities.Set(entity.entityId.id);
            entity.isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplyTransfer(this Entity entity) {
            if (entity.currentArchetypeId == 0) {
                entity.world.RemoveEntity(entity);
                return;
            }

            if (entity.previousArchetypeId != entity.currentArchetypeId) {
                if (entity.previousArchetypeId >= 0) {
                    entity.world.archetypes.data[entity.previousArchetypeId].Remove(entity);
                }
                
                entity.currentArchetype.Add(entity);
                entity.previousArchetypeId = -1;
            }

            entity.isDirty = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dispose(this Entity entity) {
            if (entity.isDisposed) {
#if MORPEH_DEBUG
                MLogger.LogError($"You're trying to dispose disposed entity with ID {entity.entityId}.");
#endif
                return;
            }
            
            entity.world.ThreadSafetyCheck();
            
            var currentArchetype = entity.currentArchetype;

            var caches = currentArchetype.world.stashes;
            foreach (var typeId in currentArchetype.typeIds) {
                if (caches.TryGetValue(typeId, out var index)) {
                    Stash.stashes.data[index].Clean(entity);
                }
            }
            
            if (entity.previousArchetypeId >= 0) {
                entity.world.archetypes.data[entity.previousArchetypeId].Remove(entity);
            }
            else {
                currentArchetype.Remove(entity);
            }

            entity.world.ApplyRemoveEntity(entity.entityId.id);
            entity.world.dirtyEntities.Unset(entity.entityId.id);

            entity.DisposeFast();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void DisposeFast(this Entity entity) {
            entity.previousArchetypeId = -1;
            entity.currentArchetypeId  = -1;

            entity.world            = null;
            entity.currentArchetype = null;

            entity.entityId   = EntityId.Invalid;
            entity.worldID    = -1;

            entity.isDirty    = false;
            entity.isDisposed = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDisposed([NotNull] this Entity entity) => entity.isDisposed || entity.entityId == EntityId.Invalid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrDisposed([CanBeNull] this Entity entity) {
            if (entity != null) {
                if (entity.world != null) {
                    entity.world.ThreadSafetyCheck();
                    if (entity.isDisposed) {
                        return true;
                    }
                    return false;
                }
                return true;
            }
            return true;
        }
    }
    namespace Experimental {
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public static class EntityExtensionsExperimental {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T As<T>(this Entity entity) where T : struct, IAspect {
#if MORPEH_DEBUG
                if (entity.IsNullOrDisposed()) {
                    throw new Exception("[MORPEH] You are trying As<T> on null or disposed entity");
                }
#endif
                var aspect = default(T);
                aspect.Entity = entity;
                return aspect;
            }
        }
    }
}
