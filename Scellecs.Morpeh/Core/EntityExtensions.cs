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
    [Obsolete("Entity extensions are obsolete and will be removed in future versions of Morpeh.")]
    public static class EntityExtensions {
#if !MORPEH_STRICT_MODE
        [Obsolete("[MORPEH] Use Stash.Add() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this Entity entity) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying AddComponent on null or disposed entity {entity}");
            }
#endif
            return ref entity.GetWorld().GetStash<T>().Add(entity);
        }

        [Obsolete("[MORPEH] Use Stash.Add() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this Entity entity, out bool exist) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying AddComponent on null or disposed entity {entity}");
            }
#endif
            return ref entity.GetWorld().GetStash<T>().Add(entity, out exist);
        }

        [Obsolete("[MORPEH] Use Stash.Get() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetComponent<T>(this Entity entity) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying GetComponent on null or disposed entity {entity}");
            }
#endif
            return ref entity.GetWorld().GetStash<T>().Get(entity);
        }

        [Obsolete("[MORPEH] Use Stash.Get() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetComponent<T>(this Entity entity, out bool exist) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying GetComponent on null or disposed entity {entity}");
            }
#endif
            return ref entity.GetWorld().GetStash<T>().Get(entity, out exist);
        }

        [Obsolete("[MORPEH] Use Stash.Set() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetComponent<T>(this Entity entity, in T value) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying SetComponent on null or disposed entity {entity}");
            }
#endif
            entity.GetWorld().GetStash<T>().Set(entity, value);
        }

        [Obsolete("[MORPEH] Use Stash.Remove() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveComponent<T>(this Entity entity) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying RemoveComponent on null or disposed entity {entity}");
            }
#endif
            return entity.GetWorld().GetStash<T>().Remove(entity);
        }

        [Obsolete("[MORPEH] Use Stash.Migrate() instead.")]
        public static void Migrate<T>(this Entity from, Entity to, bool overwrite = true) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (from.IsNullOrDisposed() || to.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying MigrateTo on null or disposed entities {from} {to}");
            }
#endif
            from.GetWorld().GetStash<T>().Migrate(from, to, overwrite);
        }
        
        [Obsolete("This method is slow and doesn't have a Stash-based alternative. Consider doing manual migration of required components.")]
        public static void MigrateTo(this Entity from, Entity to, bool overwrite = true) {
#if MORPEH_DEBUG
            if (from.IsNullOrDisposed() || to.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying MigrateTo on null or disposed entities {from} {to}");
            }
#endif

            var world = from.GetWorld();
            ref var fromEntityData = ref world.entities[from.Id];
            
            // We have to make a full copy because Migrate would modify the original data
            
            Span<StructuralChange> changes = stackalloc StructuralChange[fromEntityData.changesCount];
            for (var i = 0; i < fromEntityData.changesCount; i++) {
                changes[i] = fromEntityData.changes[i];
            }
            
            // Migrate all newly added components from transient archetype
            
            foreach (var structuralChange in changes) {
                if (!structuralChange.isAddition) {
                    continue;
                }

                world.GetExistingStash(structuralChange.typeId)?.Migrate(from, to, overwrite);
            }

            if (fromEntityData.currentArchetype == null) {
                return;
            }
            
            // Migrate all components that are not removed from the source entity from current archetype
            
            foreach (var typeId in fromEntityData.currentArchetype.components) {
                var wasRemoved = false;
                foreach (var structuralChange in changes) {
                    if (structuralChange.typeId != typeId || structuralChange.isAddition) {
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

        [Obsolete("[MORPEH] Use Stash.Has() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(this Entity entity) where T : struct, IComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                throw new Exception($"[MORPEH] You are trying Has on null or disposed entity {entity}");
            }
#endif
            return entity.GetWorld().GetStash<T>().Has(entity);
        }
#endif

        [Obsolete("[MORPEH] Use World.RemoveEntity() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Dispose(this Entity entity) {
            entity.GetWorld().RemoveEntity(entity);
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
        
        [Obsolete("[MORPEH] Store World reference in your code instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static World GetWorld(this Entity entity) {
            var worldId = entity.WorldId;
            
            if (worldId < 0 || worldId >= World.worlds.length) {
                return null;
            }
            
            return World.worlds.data[worldId];
        }
    }
}
