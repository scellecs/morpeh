#if UNITY_2019_1_OR_NEWER
namespace morpeh.Core.NativeCollections.NativeIntHashMapJobs {
    using Morpeh.Collections;
    using Unity.Collections;
    using Unity.Jobs;

    public struct TryGetIndexJob : IJob {
        [ReadOnly]
        public int input;
        [ReadOnly]
        public int capacityMinusOne;
        [ReadOnly]
        public NativeArray<IntHashMapSlot> slots;
        [ReadOnly]
        public NativeArray<int> buckets;
        [WriteOnly]
        public NativeArray<int> result;
        public void Execute() {
            var rem = this.input & this.capacityMinusOne;

            int next;
            for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                var slot = this.slots[i];
                if (slot.key - 1 == this.input) {
                    this.result[0] = i;
                    return;
                }

                next = slot.next;
            }

            this.result[0] = -1;
        }
    }
}
#endif