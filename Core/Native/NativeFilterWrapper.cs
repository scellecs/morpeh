#if MORPEH_BURST
namespace Morpeh.Native {
    using System;
    using Unity.Collections;

    public struct NativeFilterWrapper : IDisposable {
        [ReadOnly]
        public NativeArray<NativeArchetype> archetypes;
        
        [ReadOnly]
        public NativeWorld world;

        public unsafe EntityId this[int index] {
            get {
                var totalArchetypeLength = 0;
                for (int archetypeNum = 0, archetypesCount = this.archetypes.Length; archetypeNum < archetypesCount; archetypeNum++) {
                    var archetype       = this.archetypes[archetypeNum];
                    var archetypeLength = *archetype.lengthPtr;

                    if (index >= totalArchetypeLength && index < totalArchetypeLength + archetypeLength) {
                        var slotIndex = index - totalArchetypeLength;
                        var entityId = archetype.entitiesBitMap.data[slotIndex];
                        
                        return new EntityId(entityId, world.entitiesGens[entityId]);
                    }

                    totalArchetypeLength += *archetype.lengthPtr;
                }

                return EntityId.Invalid;
            }
        }

        // Dispose pattern justification: 'archetypes' is an allocated NativeArray
        public void Dispose() {
            this.archetypes.Dispose();
        }
    }
}
#endif