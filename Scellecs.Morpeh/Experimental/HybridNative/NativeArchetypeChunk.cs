using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;

namespace Scellecs.Morpeh.Experimental {
    internal unsafe struct NativeArchetypeChunk {
        internal void* data;
        internal int entitiesCount;
        internal int chunkCapacity;
        internal int componentCount;

        private NativeChunkIndex* ChunkIndices => (NativeChunkIndex*)data;
        internal NativeChunkIndex this[int index] => ChunkIndices[index];

        internal void Grow(int nextCapacity)
        {
            /*
            Assert.IsTrue(nextCapacity > chunkCapacity);

            ulong nextChunkIndicesSize = (ulong)(sizeof(NativeChunkIndex) * nextCapacity);
            ulong nextEntityCountSize = (ulong)(sizeof(int) * nextCapacity);
            ulong nextBufferSize = nextChunkIndicesSize + nextEntityCountSize;
            ulong nextBufferPtr = (ulong)UnityCollectionsBridge.Allocate((long)nextBufferSize, 16, Allocator.Persistent);

            var nextChunkData = (void*)nextBufferPtr;
            nextBufferPtr += nextChunkIndicesSize;
            uint* nextChangeVersions = (uint*)nextBufferPtr;
            nextBufferPtr += nextChangeVersionSize;
            int* nextEntityCount = (int*)nextBufferPtr;
            nextBufferPtr += nextEntityCountSize;
            int* nextSharedComponentValues = (int*)nextBufferPtr;
            nextBufferPtr += nextSharedComponentValuesSize;
            nextBufferPtr += paddingForEnabledBitAlignmentSize;
            Assert.AreEqual(0ul, nextBufferPtr & 0xF);
            byte* nextComponentEnabledBitsValues = (byte*)nextBufferPtr;
            nextBufferPtr += nextComponentEnabledBitsSize;
            int* nextComponentEnabledBitsHierarchicalDataValues = (int*)nextBufferPtr;

            int prevCount = Count;
            int prevCapacity = Capacity;
            var prevChunkData = m_Data;
            uint* prevChangeVersions = ChangeVersions;
            int* prevEntityCount = EntityCount;
            int* prevSharedComponentValues = SharedComponentValues;
            v128* prevComponentEnabledBitsValues = ComponentEnabledBits;
            int* prevComponentEnabledBitsHierarchicalDataValues = ComponentEnabledBitsHierarchicalData;

            UnsafeUtility.MemCpy(nextChunkData, prevChunkData, sizeof(ChunkIndex) * prevCount);

            for (int i = 0; i < ComponentCount; i++)
                UnsafeUtility.MemCpy(nextChangeVersions + (i * nextCapacity), prevChangeVersions + (i * prevCapacity), sizeof(uint) * Count);

            for (int i = 0; i < SharedComponentCount; i++)
                UnsafeUtility.MemCpy(nextSharedComponentValues + (i * nextCapacity), prevSharedComponentValues + (i * prevCapacity), sizeof(uint) * Count);

            UnsafeUtility.MemCpy(nextEntityCount, prevEntityCount, sizeof(int) * Count);
            UnsafeUtility.MemCpy(nextComponentEnabledBitsValues, prevComponentEnabledBitsValues, (long)ComponentEnabledBitsSize);
            UnsafeUtility.MemCpy(nextComponentEnabledBitsHierarchicalDataValues, prevComponentEnabledBitsHierarchicalDataValues, (long)ComponentEnabledBitsHierarchicalDataSize);

            UnityCollectionsBridge.Free(data, Allocator.Persistent);

            data = nextChunkData;
            chunkCapacity = nextCapacity;
            */
        }
    }
}
