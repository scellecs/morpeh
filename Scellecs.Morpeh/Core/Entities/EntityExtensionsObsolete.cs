using System;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh
{
    public static class EntityExtensionsObsolete {
#if MORPEH_ENABLE_OBSOLETE_ENTITY_DATA_COMPONENT_API
        [Obsolete("[MORPEH] Will be removed in future versions, use Stash.Add() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this Entity entity) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidAddOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            return ref entity.GetWorld().GetStash<T>().Add(entity);
        }

        [Obsolete("[MORPEH] Will be removed in future versions, use Stash.Has() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this Entity entity, out bool exist) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidAddOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            return ref entity.GetWorld().GetStash<T>().Add(entity, out exist);
        }

        [Obsolete("[MORPEH] Will be removed in future versions, use Stash.Get() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetComponent<T>(this Entity entity) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidGetOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            return ref entity.GetWorld().GetStash<T>().Get(entity);
        }

        [Obsolete("[MORPEH] Will be removed in future versions, use Stash.Has() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetComponent<T>(this Entity entity, out bool exist) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidGetOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            return ref entity.GetWorld().GetStash<T>().Get(entity, out exist);
        }

        [Obsolete("[MORPEH] Will be removed in future versions, use Stash.Has() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetComponent<T>(this Entity entity, in T value) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidSetOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            entity.GetWorld().GetStash<T>().Set(entity, value);
        }

        [Obsolete("[MORPEH] Will be removed in future versions, use Stash.Has() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveComponent<T>(this Entity entity) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidRemoveOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            return entity.GetWorld().GetStash<T>().Remove(entity);
        }
        
        [Obsolete("[MORPEH] Will be removed in future versions, use Stash.Has() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(this Entity entity) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidHasOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            return entity.GetWorld().GetStash<T>().Has(entity);
        }
#endif
    }
}