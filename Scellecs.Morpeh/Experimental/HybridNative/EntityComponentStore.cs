#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

#if UNITY_EDITOR
#define MORPEH_DEBUG
#endif

#if MORPEH_UNITY
using Scellecs.Morpeh;
using Scellecs.Morpeh.Experimental;
using Scellecs.Morpeh.Native;
using System;
using System.Diagnostics;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine.Assertions;
using static Unity.Collections.UnityCollectionsBridge;

namespace Unity.Entities
{
    // DOTS implementation
    internal unsafe struct EntityComponentStore {
        private const int DEFAULT_CAPACITY = 1024;

        internal static readonly SharedStatic<ChunkStore> chunkStore = SharedStatic<ChunkStore>.GetOrCreate<EntityComponentStore>();

        private BlockAllocator archetypeChunkAllocator;
        private UnsafePtrList<NativeArchetype> archetypes;
        private ArchetypeListMap typeLookup;

        internal byte memoryInitPattern;
        internal byte useMemoryInitPattern; // should be bool, but it doesn't get along nice with burst so far, so we use a byte instead

        internal static void Create(EntityComponentStore* entities, ulong worldSequenceNumber, int newCapacity = DEFAULT_CAPACITY) {
            ChunkStore.Intialize();
            entities->archetypes = new UnsafePtrList<NativeArchetype>(0, Allocator.Persistent);
            entities->archetypeChunkAllocator = new BlockAllocator(Unity.Collections.AllocatorManager.Persistent, 16 * 1024 * 1024);
        }

        internal struct ChunkStore : IDisposable {
            private const int LOG2_CHUNKS_PER_MEGACHUNK = 6;
            private const int CHUNKS_PER_MEGACHUNK = 1 << LOG2_CHUNKS_PER_MEGACHUNK;
            private const int LOG2_MEGACHUNKS_IN_UNIVERSE = 14;
            private const int MEGACHUNKS_IN_UNIVERSE = 1 << LOG2_MEGACHUNKS_IN_UNIVERSE;

            internal const int MAXIMUM_CHUNKS_COUNT = 1 << (LOG2_CHUNKS_PER_MEGACHUNK + LOG2_MEGACHUNKS_IN_UNIVERSE);

            private static readonly int CHUNK_SIZE_IN_BYTES_ROUNDED_TO_POW2 = math.ceilpow2(NativeChunkConstants.CHUNK_SIZE);
            private static readonly int LOG2_CHUNK_SIZE_IN_BYTES_ROUNDED_UP_TO_POW2 = math.tzcnt(CHUNK_SIZE_IN_BYTES_ROUNDED_TO_POW2);
            private static readonly int MEGACHUNK_SIZE_IN_BYTES = 1 << (LOG2_CHUNK_SIZE_IN_BYTES_ROUNDED_UP_TO_POW2 + 6);

            public const int ERROR_NONE = 0;
            public const int ERROR_ALLOCATION_FAILED = -1;
            public const int ERROR_CHUNK_ARRAY_ALREADY_FREED = -2;
            public const int ERROR_CHUNK_ALREADY_MARKED_FREE = -3;
            public const int ERROR_CHUNK_NOT_FOUND = -4;
            public const int ERROR_NO_CHUNKS_AVAILABLE = -5;

            private Ulong16384 __megachunk;
            private Ulong16384 __chunkInUse;
            private Ulong256 __megachunkIsFull;

            internal static void Intialize() {
                chunkStore.Data.__chunkInUse.ElementAt(0) = ~0L;
                chunkStore.Data.__megachunkIsFull.ElementAt(0) = 1L;
            }

            private int AllocationFailed(int offset, int count) {
                int error = ConcurrentMask.TryFree(ref __chunkInUse, offset, count);
                if (error == ConcurrentMask.ErrorFailedToFree) {
                    return ERROR_CHUNK_ARRAY_ALREADY_FREED;
                }

                return ERROR_ALLOCATION_FAILED;
            }

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS"), Conditional("MORPEH_DEBUG")]
            private void ThrowChunkAlreadyMarkedAsFree(NativeChunkIndex chunk) {
                throw new ArgumentException($"Chunk index {(int)chunk} (address {(long)chunk.ChunkPtr():x8}) already marked as free");
            }

            internal NativeChunk* GetChunkPointer(int chunkIndex) {
                var megachunkIndex = chunkIndex >> LOG2_CHUNKS_PER_MEGACHUNK;
                var chunkInMegachunk = chunkIndex & (CHUNKS_PER_MEGACHUNK - 1);
                var megachunk = (byte*)__megachunk.ElementAt(megachunkIndex);
                var chunk = megachunk + (chunkInMegachunk << LOG2_CHUNK_SIZE_IN_BYTES_ROUNDED_UP_TO_POW2);
                return (NativeChunk*)chunk;
            }

            internal int AllocateContiguousChunks(out NativeChunkIndex value, int requestedCount, out int actualCount) {
                int gigachunkIndex = 0;
                for (; gigachunkIndex < __megachunkIsFull.Length; ++gigachunkIndex) {
                    if (__megachunkIsFull.ElementAt(gigachunkIndex) != ~0L) {
                        break;
                    }
                }
                int firstMegachunk = gigachunkIndex << 6;
                actualCount = math.min(CHUNKS_PER_MEGACHUNK, requestedCount); // literally can't service requests for more
                value = NativeChunkIndex.Null;
                while (actualCount > 0) {
                    for (int offset = 0; offset < MEGACHUNKS_IN_UNIVERSE; ++offset) {
                        int megachunkIndex = (firstMegachunk + offset) & (MEGACHUNKS_IN_UNIVERSE - 1); // index of current megachunk
                        long maskAfterAllocation, oldMask, newMask, readMask = __chunkInUse.ElementAt(megachunkIndex); // read the mask of which chunks are allocated
                        int chunkInMegachunk; // index of first chunk allocated in current megachunk
                        do {
                            oldMask = readMask;
                            if (oldMask == ~0L)
                                goto NEXT_MEGACHUNK; // can't find any bits, try the next megachunk
                            if (!ConcurrentMask.foundAtLeastThisManyConsecutiveZeroes(oldMask, actualCount, out chunkInMegachunk, out int _)) // find consecutive 0 bits to allocate into
                                goto NEXT_MEGACHUNK; // can't find enough bits, try the next megachunk
                            newMask = maskAfterAllocation = oldMask | ConcurrentMask.MakeMask(chunkInMegachunk, actualCount); // mask in the freshly allocated bits
                            if (oldMask == 0L) // if we're the first to allocate from this megachunk,
                                newMask = ~0L; // mark the whole megachunk as full (busy) until we're done allocating memory
                            readMask = Interlocked.CompareExchange(ref __chunkInUse.ElementAt(megachunkIndex), newMask, oldMask);
                        } while (readMask != oldMask);
                        int firstChunkIndex = (megachunkIndex << LOG2_CHUNKS_PER_MEGACHUNK) + chunkInMegachunk;
                        if (oldMask == 0L) { // if we are the first allocation in this chunk...
                            long allocated = (long)Allocate(MEGACHUNK_SIZE_IN_BYTES, Unity.Collections.CollectionHelper.CacheLineSize, Allocator.Persistent); // allocate memory
                            if (allocated == 0L) // if the allocation failed...
                                return AllocationFailed(firstChunkIndex, actualCount);
                            Interlocked.Exchange(ref __megachunk.ElementAt(megachunkIndex), allocated); // store the pointer to the freshly allocated memory
                            Interlocked.Exchange(ref __chunkInUse.ElementAt(megachunkIndex), maskAfterAllocation); // change the mask from ~0L to the true mask after our allocation (which may be ~0L)
                        }
                        if (maskAfterAllocation == ~0L) {
                            ConcurrentMask.AtomicOr(ref __megachunkIsFull.ElementAt(megachunkIndex >> 6), 1L << (megachunkIndex & 63));
                        }
                        value = new NativeChunkIndex(firstChunkIndex);
                        return ERROR_NONE;
                    NEXT_MEGACHUNK:;
                    }
                    actualCount >>= 1;
                }
                return ERROR_NO_CHUNKS_AVAILABLE;
            }

            internal int FreeContiguousChunks(NativeChunkIndex firstChunk, int count) {
                var megachunkIndex = firstChunk >> LOG2_CHUNKS_PER_MEGACHUNK;
                var chunkIndexInMegachunk = firstChunk & (CHUNKS_PER_MEGACHUNK - 1);

                long chunksToFree = ConcurrentMask.MakeMask(chunkIndexInMegachunk, count);
                long oldMask, newMask, readMask = __chunkInUse.ElementAt(megachunkIndex); // read the mask of which chunks are allocated
                do {
                    oldMask = readMask;
                    if ((oldMask & chunksToFree) != chunksToFree) {  // if any of our chunks were already freed,
                        ThrowChunkAlreadyMarkedAsFree(firstChunk); // pretty serious error! throw,
                        return ERROR_CHUNK_ALREADY_MARKED_FREE; // and return an error code.
                    }
                    newMask = oldMask & ~chunksToFree; // zero out the chunks to free in the mask
                    if (newMask == 0L) // if this would zero out the whole mask,
                        newMask = ~0L; // *set* the whole mask.. to block new allocations from other threads until we can free the memory
                    readMask = Interlocked.CompareExchange(ref __chunkInUse.ElementAt(megachunkIndex), newMask, oldMask);
                } while (readMask != oldMask);
                if (newMask == ~0L) { // we set the whole mask, we aren't done until we free the memory and then zero the whole mask.
                    var oldMegachunk = Interlocked.Exchange(ref __megachunk.ElementAt(megachunkIndex), 0L); // set the pointer to 0.
                    Interlocked.Exchange(ref __chunkInUse.ElementAt(megachunkIndex), 0L); // set the word to 0. "come allocate from me!"
                    Free((byte*)oldMegachunk, Allocator.Persistent); // free the megachunk, since nobody can see it anymore.
                }
                ConcurrentMask.AtomicAnd(ref __megachunkIsFull.ElementAt(megachunkIndex >> 6), ~(1L << (megachunkIndex & 63)));
                return ERROR_NONE;
            }

            public void Dispose() {
                for (var megachunkIndex = 0; megachunkIndex < MEGACHUNKS_IN_UNIVERSE; ++megachunkIndex) {
                    void* megachunk = (void*)__megachunk.ElementAt(megachunkIndex);
                    if (megachunk != null) {
                        Free(megachunk, Allocator.Persistent);
                    }
                }
                this = default;
            }
        }

        internal static int AllocateContiguousChunk(int requestedCount, out NativeChunkIndex chunk, out int actualCount) {
            return chunkStore.Data.AllocateContiguousChunks(out chunk, requestedCount, out actualCount);
        }

        internal static int FreeContiguousChunks(NativeChunkIndex firstChunk, int count) {
            return chunkStore.Data.FreeContiguousChunks(firstChunk, count);
        }

        private static int CalculateChunkCapacity(int bufferSize, ushort* componentSizes, int count) {
            int totalSize = 0;
            for (int i = 0; i < count; ++i) {
                totalSize += componentSizes[i];
            }

            int capacity = bufferSize / totalSize;
            while (CalculateSpaceRequirement(componentSizes, count, capacity) > bufferSize) {
                --capacity;
            }

            return capacity;
        }

        private static int CalculateSpaceRequirement(ushort* componentSizes, int componentCount, int entityCount) {
            int size = 0;
            for (int i = 0; i < componentCount; ++i) {
                size += GetComponentArraySize(componentSizes[i], entityCount);
            }
            return size;
        }

        private static int GetComponentArraySize(int componentSize, int entityCount) {
            return Unity.Collections.CollectionHelper.Align(componentSize * entityCount, Unity.Collections.CollectionHelper.CacheLineSize);
        }

        internal NativeChunkIndex AllocateChunk() {
            if (chunkStore.Data.AllocateContiguousChunks(out var newChunk, 1, out _) != ChunkStore.ERROR_NONE) {
                throw new InvalidOperationException($"ChunkStore.AllocateContiguousChunks failed");
            }

            if (useMemoryInitPattern != 0) {
                var raw = newChunk.ChunkPtr();
                UnsafeUtility.MemSet(raw, memoryInitPattern, NativeChunkConstants.CHUNK_SIZE);
            }

            return newChunk;
        }

        public void FreeChunk(NativeChunkIndex chunk) {
            var success = chunkStore.Data.FreeContiguousChunks(chunk, 1);
            Assert.IsTrue(success == 0);
        }

        private void ChunkAllocate<T>(void* pointer, int count = 1) where T : struct {
            void** pointerToPointer = (void**)pointer;
            *pointerToPointer = archetypeChunkAllocator.Allocate(UnsafeUtility.SizeOf<T>() * count, UnsafeUtility.AlignOf<T>());
        }

        public NativeArchetype* GetExistingArchetype(ArchetypeHash hash) {
            return typeLookup.TryGet(hash);
        }

        internal NativeArchetype* CreateArchetype(int* typeIds, int count) {
            NativeArchetype* dstArchetype = null;
            ChunkAllocate<NativeArchetype>(&dstArchetype);
            ChunkAllocate<int>(&dstArchetype->offsets, count);
            ChunkAllocate<int>(&dstArchetype->sizeOfs, count);
            ChunkAllocate<int>(&dstArchetype->typeIds, count);
            dstArchetype->typesCount = count;
            dstArchetype->entityCount = 0;
            dstArchetype->chunksWithEmptySlots = new UnsafeList<NativeChunkIndex>(0, Allocator.Persistent);
            dstArchetype->nextChangedArchetype = null;
            dstArchetype->managedArchetypesRefCount = 1;

            var chunkDataSize = NativeChunkConstants.CHUNK_BUFFER_SIZE;
            var maxCapacity = NativeChunkConstants.MAX_CHUNK_CAPACITY;
            var nonZeroSizedComponents = 0;

            for (int i = 0; i < count; i++) {
                var typeId = typeIds[i];
                ComponentId.TryGetNative(typeId, out var typeInfo); // temp govno
                dstArchetype->sizeOfs[i] = (ushort)typeInfo.sizeInChunk;
                maxCapacity = math.min(maxCapacity, typeInfo.maximumChunkCapacity);
                nonZeroSizedComponents += typeInfo.sizeInChunk > 0 ? typeInfo.sizeInChunk : 0;
            }

            dstArchetype->nonZeroSizedTypesCount = nonZeroSizedComponents;
            dstArchetype->chunkCapacity = math.min(maxCapacity, CalculateChunkCapacity(chunkDataSize, dstArchetype->sizeOfs, nonZeroSizedComponents)); {
                Assert.IsTrue(dstArchetype->chunkCapacity > 0);
                Assert.IsTrue(NativeChunkConstants.MAX_CHUNK_CAPACITY >= dstArchetype->chunkCapacity);
            }

            dstArchetype->chunks = new NativeArchetypeChunk() {
                data = null,
                chunkCapacity = 0,
                entitiesCount = 0,
                componentCount = count,
            };

            dstArchetype->instanceSize = 0;
            dstArchetype->instanceSizeWithOverhead = 0;
            for (var i = 0; i < nonZeroSizedComponents; ++i) {
                dstArchetype->instanceSize += dstArchetype->sizeOfs[i];
                dstArchetype->instanceSizeWithOverhead += GetComponentArraySize(dstArchetype->sizeOfs[i], 1);
            }

            for (int i = 0, usedBytes = 0; i < count; i++) {
                dstArchetype->typeIds[i] = typeIds[i];
                dstArchetype->offsets[i] = usedBytes;
                usedBytes += GetComponentArraySize(dstArchetype->sizeOfs[i], dstArchetype->chunkCapacity);
            }

            archetypes.Add(dstArchetype);
            typeLookup.Add(dstArchetype);

            fixed (EntityComponentStore* entityComponentStore = &this) {
                dstArchetype->entityComponentStore = entityComponentStore;
            }

            return dstArchetype;
        }

#pragma warning disable 169
        struct Ulong16 {
            private ulong p00;
            private ulong p01;
            private ulong p02;
            private ulong p03;
            private ulong p04;
            private ulong p05;
            private ulong p06;
            private ulong p07;
            private ulong p08;
            private ulong p09;
            private ulong p10;
            private ulong p11;
            private ulong p12;
            private ulong p13;
            private ulong p14;
            private ulong p15;
        }

        struct Ulong256 {
            private Ulong16 p00;
            private Ulong16 p01;
            private Ulong16 p02;
            private Ulong16 p03;
            private Ulong16 p04;
            private Ulong16 p05;
            private Ulong16 p06;
            private Ulong16 p07;
            private Ulong16 p08;
            private Ulong16 p09;
            private Ulong16 p10;
            private Ulong16 p11;
            private Ulong16 p12;
            private Ulong16 p13;
            private Ulong16 p14;
            private Ulong16 p15;
            public int Length { get { return 256; } set { } }
            public ref long ElementAt(int index) {
                fixed (Ulong16* p = &p00) { return ref UnsafeUtility.AsRef<long>((long*)p + index); }
            }
        }

        struct Ulong4096 {
            private Ulong256 p00;
            private Ulong256 p01;
            private Ulong256 p02;
            private Ulong256 p03;
            private Ulong256 p04;
            private Ulong256 p05;
            private Ulong256 p06;
            private Ulong256 p07;
            private Ulong256 p08;
            private Ulong256 p09;
            private Ulong256 p10;
            private Ulong256 p11;
            private Ulong256 p12;
            private Ulong256 p13;
            private Ulong256 p14;
            private Ulong256 p15;
        }

        struct Ulong16384 : IIndexable<long> {
            private Ulong4096 p00;
            private Ulong4096 p01;
            private Ulong4096 p02;
            private Ulong4096 p03;
            public int Length { get { return 16384; } set { } }
            public ref long ElementAt(int index) {
                fixed (Ulong4096* p = &p00) { return ref UnsafeUtility.AsRef<long>((long*)p + index); }
            }
        }
#pragma warning restore 169
    }
}
#endif