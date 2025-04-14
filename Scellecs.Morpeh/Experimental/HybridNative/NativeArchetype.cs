using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Scellecs.Morpeh.Experimental {
    internal unsafe struct NativeArchetype {
        internal NativeArchetypeChunk chunks;
        internal UnsafeList<NativeChunkIndex> chunksWithEmptySlots;
        internal NativeArchetype* nextChangedArchetype;
        internal int* offsets;
        internal ushort* sizeOfs;
        internal int* typeIds;
        internal EntityComponentStore* entityComponentStore;
        internal int entityCount;
        internal int chunkCapacity;
        internal int typesCount;
        internal int nonZeroSizedTypesCount;
        internal int instanceSize;
        internal int instanceSizeWithOverhead;
        internal int managedArchetypesRefCount;
        internal ArchetypeHash hash;
    }
}
