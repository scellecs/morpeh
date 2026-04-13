using System;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh
{
    public static class EntityExtensionsObsolete {
#if !MORPEH_STRICT_MODE
        [Obsolete("[MORPEH] Use Stash.Add() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this Entity entity) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidAddOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            return ref entity.GetWorld().GetStash<T>().Add(entity);
        }

        [Obsolete("[MORPEH] Use Stash.Add() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T AddComponent<T>(this Entity entity, out bool exist) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidAddOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            return ref entity.GetWorld().GetStash<T>().Add(entity, out exist);
        }

        [Obsolete("[MORPEH] Use Stash.Get() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetComponent<T>(this Entity entity) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidGetOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            return ref entity.GetWorld().GetStash<T>().Get(entity);
        }

        [Obsolete("[MORPEH] Use Stash.Get() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetComponent<T>(this Entity entity, out bool exist) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidGetOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            return ref entity.GetWorld().GetStash<T>().Get(entity, out exist);
        }

        [Obsolete("[MORPEH] Use Stash.Set() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetComponent<T>(this Entity entity, in T value) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidSetOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            entity.GetWorld().GetStash<T>().Set(entity, value);
        }

        [Obsolete("[MORPEH] Use Stash.Remove() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveComponent<T>(this Entity entity) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidRemoveOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            return entity.GetWorld().GetStash<T>().Remove(entity);
        }
        
        [Obsolete("[MORPEH] Use Stash.Has() instead.")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(this Entity entity) where T : struct, IDataComponent {
#if MORPEH_DEBUG
            if (entity.IsNullOrDisposed()) {
                InvalidHasOperationException.ThrowDisposedEntity(entity, typeof(T));
            }
#endif
            return entity.GetWorld().GetStash<T>().Has(entity);
        }
    }
#endif
}