#if UNITY_2019_1_OR_NEWER
namespace morpeh.Core.NativeCollections.NativeIntHashMapJobs {
    using Morpeh.Collections;
    using Unity.Collections;
    using Unity.Jobs;

    public struct GetAllIndicesAndAssignJob : IJob {
        [ReadOnly]
        public NativeArray<int> inputs;
        [ReadOnly]
        public int capacityMinusOne;
        [ReadOnly]
        public NativeArray<IntHashMapSlot> slots;
        [ReadOnly]
        public NativeArray<int> buckets;
        [WriteOnly]
        public NativeArray<int> results;
        public void Execute() {
            for (int i = 0, length = this.inputs.Length; i < length; i++) {
                this.results[i] = this.FindByEntityId(this.inputs[i]);
            }
        }

        private int FindByEntityId(int key) {
            var rem = key & this.capacityMinusOne;

            int next;
            for (var i = this.buckets[rem] - 1; i >= 0; i = next) {
                var slot = this.slots[i];
                if (slot.key - 1 == key) {
                    return i;
                }

                next = slot.next;
            }

            return -1;
        }
    }
}
#endif