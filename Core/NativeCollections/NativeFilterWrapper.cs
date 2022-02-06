#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    using System;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;

    public struct NativeFilterWrapper : IDisposable {
        public NativeArray<NativeArchetypeWrapper> archetypes;

        [NativeDisableUnsafePtrRestriction]
        public unsafe int* lengthPtr;

        public unsafe int Length => *this.lengthPtr;

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

        public void Dispose() {
            this.archetypes.Dispose();
        }
    }
}
#endif