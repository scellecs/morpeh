#if UNITY_2019_1_OR_NEWER
namespace morpeh.Core.NativeCollections {
    using System.Runtime.CompilerServices;
    using Morpeh.Collections;
    using NativeIntHashMapJobs;
    using Unity.Collections;
    using Unity.Jobs;

    public struct NativeIntHashMap<TNative> where TNative : unmanaged {
        public unsafe int* lengthPtr;
        public unsafe int* capacityPtr;
        public unsafe int* capacityMinusOnePtr;
        public unsafe int* lastIndexPtr;
        public unsafe int* freeIndexPtr;
        
        public NativeArray<int>     buckets;
        public NativeArray<IntHashMapSlot>    slots;
        public NativeArray<TNative> data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe NativeArray<int> GetAllIndicesAndAssign(in NativeArray<int> keys) {
            var job = new GetAllIndicesAndAssignJob {
                inputs           = keys,
                capacityMinusOne = *this.capacityMinusOnePtr,
                slots            = this.slots,
                buckets          = this.buckets,
                results          = new NativeArray<int>(keys.Length, Allocator.TempJob)
            };

            var handle = job.Schedule();
            handle.Complete();
            return job.results;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int TryGetIndex(in int key) {
            var job = new TryGetIndexJob {
                input            = key,
                capacityMinusOne = *this.capacityMinusOnePtr,
                slots            = this.slots,
                buckets          = this.buckets,
                result           = new NativeArray<int>(1, Allocator.TempJob)
            };

            var handle = job.Schedule();
            handle.Complete();

            var result = job.result[0];
            job.result.Dispose();
            return result;
        }
    }
}
#endif