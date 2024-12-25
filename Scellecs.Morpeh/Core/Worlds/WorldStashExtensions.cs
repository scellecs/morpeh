namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using Scellecs.Morpeh.Collections;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class WorldStashExtensions {
        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IStash GetExistingStash(this World world, int typeId) {
            world.ThreadSafetyCheck();
            
            if (typeId < 0 || typeId >= world.stashes.Length) {
                return null;
            }
            
            return world.stashes[typeId];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        public static IStash GetReflectionStash(this World world, Type type) {
            world.ThreadSafetyCheck();

            if (!ComponentId.TryGet(type, out var definition)) {
                return world.CreateReflectionStash(type);
            }

            var candidate = world.GetExistingStash(definition.id);
            return candidate ?? world.CreateReflectionStash(type);
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IStash CreateReflectionStash(this World world, Type type)
        {
            var createMethod = type.GetMethod("GetStash", new[] { typeof(World), });
            var stash        = (IStash)createMethod?.Invoke(null, new object[] { world, });
            
            var definition = ComponentId.Get(type);
            
            world.EnsureStashCapacity(definition.id);
            world.stashes[definition.id] = stash;
            
            return stash;
        }
        
        // TODO: Change to IDataComponent. This will be a breaking change, proceed with caution.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        [UnityEngine.Scripting.Preserve]
        public static Stash<T> GetStash<T>(this World world, int capacity = -1) where T : struct, IComponent {
            world.ThreadSafetyCheck();
            
            var info = ComponentId<T>.info;

            var candidate = world.GetExistingStash(info.id);
            if (candidate != null) {
                if (candidate is Stash<T> typeStash) {
                    return typeStash;
                } else {
                    throw new InvalidOperationException($"Stash {candidate.Type} already exists, but with different Stash type.");
                }
            }
            
            world.EnsureStashCapacity(info.id);
            
            var stash = new Stash<T>(world, info, capacity);
            world.stashes[info.id] = stash;
            return stash;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        [UnityEngine.Scripting.Preserve]
        public static DisposableStash<T> GetDisposableStash<T>(this World world, int capacity = -1) where T : struct, IDisposableComponent {
            world.ThreadSafetyCheck();
            
            var info = ComponentId<T>.info;

            var candidate = world.GetExistingStash(info.id);
            if (candidate != null) {
                if (candidate is DisposableStash<T> typeStash) {
                    return typeStash;
                } else {
                    throw new InvalidOperationException($"Stash {candidate.Type} already exists, but with different Stash type.");
                }
            }
            
            world.EnsureStashCapacity(info.id);
            
            var stash = new DisposableStash<T>(world, info, capacity);
            world.stashes[info.id] = stash;
            return stash;
        }
        
        // TODO: Pass type + info externally to avoid extra generic method which is absolutely useless
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        [UnityEngine.Scripting.Preserve]
        public static TagStash GetTagStash<T>(this World world, int capacity = -1) where T : struct, ITagComponent {
            world.ThreadSafetyCheck();
            
            var info = ComponentId<T>.info;

            var candidate = world.GetExistingStash(info.id);
            if (candidate != null) {
                if (candidate is TagStash typeStash) {
                    return typeStash;
                } else {
                    throw new InvalidOperationException($"Stash {candidate.Type} already exists, but with different Stash type.");
                }
            }
            
            world.EnsureStashCapacity(info.id);
            
            var stash = new TagStash(world, typeof(T), info, capacity);
            world.stashes[info.id] = stash;
            return stash;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EnsureStashCapacity(this World world, int capacity) {
            var newSize = world.stashes.Length;
            while (capacity >= newSize) {
                newSize <<= 1;
            }
            
            if (newSize > world.stashes.Length) {
                world.GrowStashCapacity(newSize);
            }
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void GrowStashCapacity(this World world, int newCapacity) {
            ArrayHelpers.Grow(ref world.stashes, newCapacity);
        }
    }
}