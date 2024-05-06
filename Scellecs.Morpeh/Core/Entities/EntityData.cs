namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct EntityData {
        internal Archetype currentArchetype;
        internal int indexInCurrentArchetype;
        
        internal ArchetypeHash nextArchetypeHash;
        
        internal int[] addedComponents;
        internal ushort addedComponentsCount;
        
        internal int[] removedComponents;
        internal ushort removedComponentsCount;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Initialize() {
            this.currentArchetype = null;
            this.indexInCurrentArchetype = -1;
            this.nextArchetypeHash = default;
            
            this.addedComponents = new int[8];
            this.addedComponentsCount = 0;
            
            this.removedComponents = new int[8];
            this.removedComponentsCount = 0;
        }
    }
}