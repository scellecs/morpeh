#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System;
    using Unity.Collections;
    using Unity.Jobs;

    public struct NativeFilter : IDisposable {
        [ReadOnly]
        public int length;

        [ReadOnly]
        internal NativeArray<NativeArchetype> archetypes;
        
        [ReadOnly]
        internal NativeWorld world;

        public unsafe EntityId this[int index] {
            get {
                var totalArchetypeLength = 0;
                for (int archetypeNum = 0, archetypesCount = this.archetypes.Length; archetypeNum < archetypesCount; archetypeNum++) {
                    var archetype       = this.archetypes[archetypeNum];
                    var archetypeLength = *archetype.lengthPtr;

                    if (index >= totalArchetypeLength && index < totalArchetypeLength + archetypeLength) {
                        var slotIndex = index - totalArchetypeLength;
                        var entityId = archetype.entities.data[slotIndex];
                        
                        return new EntityId(entityId, this.world.entitiesGens[entityId]);
                    }

                    totalArchetypeLength += archetypeLength;
                }

                return EntityId.Invalid;
            }
        }

        // Dispose pattern justification: 'archetypes' is an allocated NativeArray
        public void Dispose() {
            this.archetypes.Dispose();
        }
        
        public JobHandle Dispose(JobHandle inputDeps) {
            return this.archetypes.Dispose(inputDeps);
        }
    }
}
#endif