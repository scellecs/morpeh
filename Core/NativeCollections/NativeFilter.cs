#if MORPEH_BURST
namespace Morpeh.NativeCollections {
    using System;
    using Unity.Collections;

    public struct NativeFilter : IDisposable {
        internal NativeFilter(NativeFilterWrapper filterWrapper, int length) {
            this.filterWrapper = filterWrapper;
            this.length        = length;
        }
        
        [ReadOnly]
        public readonly int length;

        [ReadOnly]
        private NativeFilterWrapper filterWrapper;

        public int this[int index] => this.filterWrapper[index];
        
        // Dispose pattern justification: 'archetypes' in 'filterWrapper' is an allocated NativeArray
        public void Dispose() {
            this.filterWrapper.Dispose();
        }
    }
}
#endif