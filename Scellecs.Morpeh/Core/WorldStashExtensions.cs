namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    
    public static class WorldStashExtensions {
        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Stash GetStash(this World world, int offset) {
            world.ThreadSafetyCheck();
            
            if (offset < 0 || offset >= world.stashes.Length) {
                return null;
            }
            
            return world.stashes[offset];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        public static Stash GetReflectionStash(this World world, Type type) {
            world.ThreadSafetyCheck();
            
            if (TypeIdentifier.typeAssociation.TryGetValue(type, out var definition)) {
                var candidate = world.GetStash(definition.offset.GetValue());
                
                if (candidate != null) {
                    return candidate;
                }
            }

            var stash = Stash.CreateReflection(world, type);
            TypeIdentifier.typeAssociation.TryGetValue(type, out definition);
            
            world.EnsureStashCapacity(definition.offset.GetValue());
            world.stashes[definition.offset.GetValue()] = stash;

            return stash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [PublicAPI]
        public static Stash<T> GetStash<T>(this World world) where T : struct, IComponent {
            world.ThreadSafetyCheck();
            
            var info = TypeIdentifier<T>.info;
            var offset = info.offset.GetValue();
            
            var candidate = world.GetStash(offset);
            if (candidate != null) {
                return (Stash<T>)candidate.typelessStash;
            }

            var stash = Stash.Create<T>(world);
            
            world.EnsureStashCapacity(offset);
            world.stashes[offset] = stash;

            return (Stash<T>)stash.typelessStash;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EnsureStashCapacity(this World world, int capacity) {
            var newSize = world.stashes.Length;
            while (capacity >= newSize) {
                newSize = newSize << 1;
            }
            
            if (newSize > world.stashes.Length) {
                Array.Resize(ref world.stashes, newSize);
            }
        }
    }
}