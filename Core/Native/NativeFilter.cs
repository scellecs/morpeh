#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using Unity.Collections;

    public unsafe struct NativeFilter {
        [ReadOnly]
        public int length;

        [ReadOnly]
        public NativeFastList<Filter.Chunk> chunks;
        
        [ReadOnly]
        public NativeWorld world;

        public EntityId this[int index] {
            get {
                var totalChunkLength = 0;
                for (int archetypeNum = 0, archetypesCount = *this.chunks.lengthPtr; archetypeNum < archetypesCount; archetypeNum++) {
                    var chunk       = this.chunks.data[archetypeNum];
                    var chunkLength = chunk.entitiesLength;

                    if (index >= totalChunkLength && index < totalChunkLength + chunkLength) {
                        var slotIndex = index - totalChunkLength;
                        var entityId = chunk.entities[slotIndex];
                        
                        return new EntityId(entityId, this.world.entitiesGens[entityId]);
                    }

                    totalChunkLength += chunkLength;
                }

                return EntityId.Invalid;
            }
        }
    }
}
#endif