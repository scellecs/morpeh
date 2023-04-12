#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Collections;

    public static class NativeArchetypeExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe Filter.Chunk AsChunk(this Archetype archetype) {
            if (archetype.usedInNative == false) {
                var data = archetype.entitiesNative = new UnsafeFastList<int>(archetype.entities.count);

                foreach (var entityId in archetype.entities) {
                    archetype.world.entities[entityId].indexInCurrentArchetype = data.Add(entityId);
                }

                archetype.entities     = null;
                archetype.usedInNative = true;
            }
            
            var chunk = new Filter.Chunk {
                entities = archetype.entitiesNative.data.ptr,
                entitiesLength = archetype.entitiesNative.length
            };
            
            return chunk;
        }
    }
}
#endif