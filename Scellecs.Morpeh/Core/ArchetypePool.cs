namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Scellecs.Morpeh.Collections;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
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
            
            foreach (var idx in archetype.filters) {
                var filter = archetype.filters.GetValueByIndex(idx);
                filter.RemoveArchetype(archetype);
            }
            archetype.ClearFilters();
            archetype.components.Clear();
            
            archetype.id = ArchetypeId.Invalid;
        }
    }
}