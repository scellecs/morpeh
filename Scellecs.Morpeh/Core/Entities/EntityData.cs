namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct EntityData {
        internal Archetype     currentArchetype;
        internal ArchetypeHash nextArchetypeHash;
        
        internal int indexInCurrentArchetype;
        internal int addedComponentsCount;
        internal int removedComponentsCount;
        
        internal int[] addedComponents;
        internal int[] removedComponents;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Initialize() {
            this.currentArchetype  = null;
            this.nextArchetypeHash = default;
            
            this.indexInCurrentArchetype = -1;
            this.addedComponentsCount    = 0;
            this.removedComponentsCount  = 0;
            
            this.addedComponents = new int[8];
            this.removedComponents = new int[8];
        }
    }
}