#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using Unity.Collections;

    // TODO: Add Enumerator
    public unsafe struct NativeFilter {
        [ReadOnly]
        public int length;

        [ReadOnly]
        public NativeFastList<Filter.Chunk> archetypes;
        
        [ReadOnly]
        public NativeWorld world;

        public Entity this[int index] {
            get {
                var totalArchetypeLength = 0;
                for (int archetypeNum = 0, archetypesCount = *this.archetypes.lengthPtr; archetypeNum < archetypesCount; archetypeNum++) {
                    var archetype = this.archetypes.data[archetypeNum];
                    var archetypeLength = archetype.entitiesLength;

                    if (index >= totalArchetypeLength && index < totalArchetypeLength + archetypeLength) {
                        var slotIndex = index - totalArchetypeLength;
                        return archetype.entities[slotIndex];
                    }

                    totalArchetypeLength += archetypeLength;
                }

                return default;
            }
        }
    }
}
#endif