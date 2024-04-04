namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using Scellecs.Morpeh.Collections;
    
    public static class WorldStashExtensions {
        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IStash GetStash(this World world, int typeId) {
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
            
            if (TypeIdentifier.typeAssociation.TryGetValue(type, out var definition)) {
                var candidate = world.GetStash(definition.id);
                
                if (candidate != null) {
                    return candidate;
                }
            }

            var createMethod = typeof(WorldStashExtensions).GetMethod("GetStash", new[] { typeof(World), });
            var genericMethod = createMethod?.MakeGenericMethod(type);
            var stash = (IStash)genericMethod?.Invoke(null, new object[] { world, });
            
            TypeIdentifier.typeAssociation.TryGetValue(type, out definition);
            
            world.EnsureStashCapacity(definition.id);
            world.stashes[definition.id] = stash;

            return stash;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        [UnityEngine.Scripting.Preserve]
        public static Stash<T> GetStash<T>(this World world) where T : struct, IComponent {
            world.ThreadSafetyCheck();
            
            var info = TypeIdentifier<T>.info;
            
            var candidate = world.GetStash(info.id);
            if (candidate != null) {
                return (Stash<T>)candidate;
            }
            
            world.EnsureStashCapacity(info.id);
            
            var stash = new Stash<T>(world, info);
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
                ArrayHelpers.Grow(ref world.stashes, newSize);
            }
        }
    }
}