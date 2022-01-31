namespace morpeh.Core.NativeCollections {
    using System;
    using Unity.Collections;

    public struct NativeFilter : IDisposable {
        public        NativeArray<NativeArchetype> archetypes;
        public unsafe int*                    LengthPtr;

        public void Dispose() {
            this.archetypes.Dispose();
        }
    }
}