using System.Runtime.CompilerServices;
using Scellecs.Morpeh.Collections;

namespace Scellecs.Morpeh {
    internal class ArchetypePool {
        private FastList<Archetype> archetypes;
        
        public ArchetypePool(int initialCapacity = 32) {
            this.archetypes = new FastList<Archetype>(initialCapacity);
            WarmUp(initialCapacity);
        }
        
        public void WarmUp(int count) {
            for (var i = 0; i < count; i++) {
                this.archetypes.Add(new Archetype(ArchetypeId.Invalid));
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Archetype Rent(ArchetypeId archetypeId) {
            if (this.archetypes.length == 0) {
                return new Archetype(archetypeId);
            }
            
            var index = this.archetypes.length - 1;
            
            var archetype = this.archetypes.data[index];
            this.archetypes.RemoveAt(index);
            
            archetype.id = archetypeId;
            
            return archetype;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(Archetype archetype) {
            this.archetypes.Add(archetype);
            
            foreach (var filter in archetype.filters) {
                filter.RemoveArchetype(archetype);
            }
            archetype.ClearFilters();
            archetype.components.Clear();
            
            archetype.id = ArchetypeId.Invalid;
        }
    }
}