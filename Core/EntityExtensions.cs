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
        internal static Entity Create(int id, World world) {
            var newEntity = new Entity { 
                entityId = new EntityId(id, world.entitiesGens[id]), 
                world = world,
                components = new SortedBitMap()
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
            var stash = entity.world.GetStash<T>();

            return ref stash.Add(entity);
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
            var stash = entity.world.GetStash<T>();

            return ref stash.Add(entity, out exist);
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
            var stash = entity.world.GetStash<T>();

            return ref stash.Get(entity);
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
            var stash = entity.world.GetStash<T>();

            return ref stash.Get(entity, out exist);
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
            var stash = entity.world.GetStash<T>();

            stash.Set(entity, value);
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
            var stash = entity.world.GetStash<T>();

            return stash.Remove(entity);
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

            var stash = entity.world.GetStash<T>();
            return stash.Has(entity);
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

            foreach (var offset in from.components) {
                var id = CommonTypeIdentifier.offsetTypeAssociation[offset].id;
                var stash = Stash.stashes.data[world.stashes.GetValueByKey(id)];
                stash.Migrate(from, to, overwrite);
            }
        }
        
#endif
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddTransfer(this Entity entity, long typeId, int offset) {
            if (entity.previousArchetypeLength == 0) {
                entity.previousArchetype = entity.currentArchetype;
                entity.previousArchetypeLength = entity.currentArchetypeLength;
            }
            
            entity.components.Set(offset);
            
            entity.currentArchetype ^= typeId;
            entity.currentArchetypeLength++;
            
            if (entity.isDirty == true) {
                return;
            }
            
            entity.world.dirtyEntities.Set(entity.entityId.id);
            entity.isDirty = true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void RemoveTransfer(this Entity entity, long typeId, int offset) {
            if (entity.previousArchetypeLength == 0) {
                entity.previousArchetype = entity.currentArchetype;
                entity.previousArchetypeLength = entity.currentArchetypeLength;
            }

            entity.components.Unset(offset);

            entity.currentArchetype ^= typeId;
            entity.currentArchetypeLength--;
            if (entity.isDirty == true) {
                return;
            }

            entity.world.dirtyEntities.Set(entity.entityId.id);
            entity.isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ApplyTransfer(this Entity entity) {
            if (entity.currentArchetypeLength == 0) {
                entity.world.RemoveEntity(entity);
                return;
            }

            if (entity.previousArchetype != entity.currentArchetype || entity.previousArchetypeLength != entity.currentArchetypeLength) {
                if (entity.previousArchetypeLength > 0) {
                    if (entity.world.archetypes.TryGetValue(entity.previousArchetype, out var prev)) {
                        prev.Remove(entity);
                    }
                    entity.previousArchetype = 0;
                    entity.previousArchetypeLength = 0;
                }

                if (entity.world.archetypes.TryGetValue(entity.currentArchetype, out var current)) {
                    current.Add(entity);
                }
                else {
                    CreateArchetype(entity);
                }
            }

            entity.isDirty = false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void CreateArchetype(Entity entity) {
            var world = entity.world;
            var key = entity.currentArchetype;
            
            Archetype arch;
            if (world.emptyArchetypes.length > 0) {
                var id = world.emptyArchetypes.length - 1;
                arch = world.emptyArchetypes.data[id];
                world.emptyArchetypes.RemoveAt(id);
                arch.id = key;
            }
            else {
                arch = new Archetype(key, world);
            }
            arch.Add(entity);
            
            void TreeStep(Archetype a, LongHashMap<FilterNode> tree, SortedBitMap.Enumerator components, int start, int end) {
                var c = components;
                for (int i = start; i < end; i++) {
                    c.MoveNext();
                    var offset = c.current;
                    if (tree.TryGetValue(offset, out var node)) {
                        foreach (var filter in node.filters) {
                            filter.AddArchetype(a, entity);
                        }
                        if (node.nodes != null) {
                            TreeStep(a, node.nodes, c, i + 1, end);
                        }
                    }
                }
            }
            
            TreeStep(arch, world.filtersTree, entity.components.GetEnumerator(), 0, entity.currentArchetypeLength);
            
            world.archetypes.Add(key, arch, out _);
            world.archetypesCount++;
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
            
            if (entity.currentArchetypeLength > 0) {
                foreach (var offset in entity.components) {
                    var id = CommonTypeIdentifier.offsetTypeAssociation[offset].id;
                    var stash = Stash.stashes.data[entity.world.stashes.GetValueByKey(id)];
                    stash.Clean(entity);
                }
            }

            if (entity.previousArchetypeLength > 0) {
                entity.world.archetypes.GetValueByKey(entity.previousArchetype)?.Remove(entity);
            }
            else {
                entity.world.archetypes.GetValueByKey(entity.currentArchetype)?.Remove(entity);
            }

            entity.world.ApplyRemoveEntity(entity.entityId.id);
            entity.world.dirtyEntities.Unset(entity.entityId.id);

            entity.DisposeFast();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void DisposeFast(this Entity entity) {
            entity.previousArchetype = 0;
            entity.currentArchetype = 0;
            entity.previousArchetypeLength = 0;
            entity.currentArchetypeLength = 0;

            entity.world      = null;
            entity.entityId   = EntityId.Invalid;

            entity.isDirty    = false;
            entity.isDisposed = true;
        }

        internal static void DumpHead(this Entity entity) {
            foreach (var offset in entity.components) {
                MLogger.Log(offset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDisposed([NotNull] this Entity entity) => entity.isDisposed || entity.entityId == EntityId.Invalid;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrDisposed([CanBeNull] this Entity entity) {
            return entity == null || entity.isDisposed || entity.entityId == EntityId.Invalid || entity.world == null;
        }
    }
}
