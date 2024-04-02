namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Scellecs.Morpeh.Collections;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal struct ArchetypePool {
        private FastList<Archetype> archetypes;
        
        public ArchetypePool(int initialCapacity) {
            this.archetypes = new FastList<Archetype>(initialCapacity);
            WarmUp(initialCapacity);
        }
        
        public void WarmUp(int count) {
            for (var i = 0; i < count; i++) {
                this.archetypes.Add(new Archetype(default));
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Archetype Rent(ArchetypeHash archetypeHash) {
            if (this.archetypes.length == 0) {
                return new Archetype(archetypeHash);
            }
            
            var index = this.archetypes.length - 1;
            
            var archetype = this.archetypes.data[index];
            this.archetypes.RemoveAt(index);
            
            archetype.hash = archetypeHash;
            
            return archetype;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(Archetype archetype) {
            this.archetypes.Add(archetype);
            archetype.hash = default;
        }
    }
}