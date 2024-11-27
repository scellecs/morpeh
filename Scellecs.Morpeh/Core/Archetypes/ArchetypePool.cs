namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Scellecs.Morpeh.Collections;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal struct ArchetypePool {
        internal Archetype[] archetypes;
        internal int         count;
        
        public ArchetypePool(int initialCapacity) {
            this.archetypes = new Archetype[initialCapacity];
            this.count      = 0;
            
            this.WarmUp(initialCapacity);
        }
        
        public void WarmUp(int value) {
            var newCount = this.count + value;
            
            if (newCount > this.archetypes.Length) {
                this.GrowArchetypes(newCount);
            }
            
            for (var i = this.count; i < newCount; i++) {
                this.archetypes[i] = new Archetype(default);
            }
            
            this.count = newCount;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Archetype Rent(ArchetypeHash archetypeHash) {
            if (this.count == 0) {
                return this.NewArchetype(archetypeHash);
            }
            
            this.count--;
            
            var archetype = this.archetypes[this.count];
            this.archetypes[this.count] = default;
            
            archetype.hash = archetypeHash;
            return archetype;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(Archetype archetype) {
            if (this.count == this.archetypes.Length) {
                this.GrowArchetypes(this.count << 1);
            }
            
            this.archetypes[this.count++] = archetype;
            archetype.hash = default;
        }
        
        public void Dispose() {
            for (var i = 0; i < this.count; i++) {
                this.archetypes[i].Dispose();
            }
            
            this.archetypes = default;
            this.count      = 0;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private Archetype NewArchetype(ArchetypeHash hash) => new Archetype(hash);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void GrowArchetypes(int newSize) {
            ArrayHelpers.Grow(ref this.archetypes, newSize);
        }
    }
}