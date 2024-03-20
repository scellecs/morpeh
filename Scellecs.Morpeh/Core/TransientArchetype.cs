namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    using Collections;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal struct TransientArchetype {
        internal ArchetypeId nextArchetypeId;
        
        internal IntHashMap<TypeInfo> addedComponents;
        internal IntHashMap<TypeInfo> removedComponents;
        
        internal Archetype baseArchetype;
        
        public bool IsEmpty {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.addedComponents.length + this.removedComponents.length == 0;
        }
    }
}