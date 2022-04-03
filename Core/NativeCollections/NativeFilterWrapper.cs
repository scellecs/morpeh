#if MORPEH_BURST
namespace Morpeh.NativeCollections {
    using System;
    using Unity.Collections;

    public struct NativeFilterWrapper : IDisposable {
        [ReadOnly]
        public NativeArray<NativeArchetypeWrapper> archetypes;

        public unsafe int this[int index] {
            get {
                var totalArchetypeLength = 0;
                for (int archetypeNum = 0, archetypesCount = this.archetypes.Length; archetypeNum < archetypesCount; archetypeNum++) {
                    var archetype       = this.archetypes[archetypeNum];
                    var archetypeLength = *archetype.lengthPtr;

                    if (index >= totalArchetypeLength && index < totalArchetypeLength + archetypeLength) {
                        var slotIndex = index - totalArchetypeLength;
                        return archetype.entitiesBitMap.data[slotIndex];
                    }

                    totalArchetypeLength += *archetype.lengthPtr;
                }

                return -1;
            }
        }

        // Dispose pattern justification: 'archetypes' is an allocated NativeArray
        public void Dispose() {
            this.archetypes.Dispose();
        }
    }
}
#endif