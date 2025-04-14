#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

#if MORPEH_UNITY
namespace Scellecs.Morpeh.Experimental {
    internal unsafe static class ChunkDataUtility {
        internal static void Move(int entityId, NativeArchetype* dstArchetype) { 
            
        }

        /*
        public static void Clone(NativeArchetype* srcArchetype, in EntityBatchInChunk srcBatch, NativeArchetype* dstArchetype, NativeChunkIndex dstChunk) {
            var srcChunk = srcBatch.Chunk;
            var srcChunkIndex = srcBatch.StartIndex;
            var srcCount = srcBatch.Count;
            var entityComponentStore = dstArchetype->entityComponentStore;
            var globalSystemVersion = entityComponentStore->GlobalSystemVersion;

            var dstCount = AllocateIntoChunk(dstArchetype, dstChunk, srcCount, out var dstChunkIndex);
            Assert.IsTrue(dstCount == srcCount);

            Convert(srcArchetype, srcChunk, srcChunkIndex, dstArchetype, dstChunk, dstChunkIndex, dstCount);

            var dstEntities = (Entity*)ChunkDataUtility.GetComponentDataRO(dstChunk, dstArchetype, dstChunkIndex, 0);
            for (int i = 0; i < dstCount; i++)
            {
                var entity = dstEntities[i];
                entityComponentStore->SetEntityInChunk(entity, new EntityInChunk { Chunk = dstChunk, IndexInChunk = dstChunkIndex + i });
            }

            // Can't update these counts until the chunk count is up to date and padding bits are clear
            //UpdateChunkDisabledEntityCounts(dstChunk, dstArchetype);

            dstArchetype->chunks.SetOrderVersion(dstChunk.ListIndex, globalSystemVersion);
            entityComponentStore->IncrementComponentTypeOrderVersion(dstArchetype);

            // Cannot DestroyEntities unless CleanupComplete on the entity chunk.
            if (dstArchetype->CleanupComplete)
                entityComponentStore->DestroyEntities(dstEntities, dstCount);
        }
        */
    }
}
#endif
