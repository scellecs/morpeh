namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct EntityData {
        internal Archetype currentArchetype;
        internal int indexInCurrentArchetype;
        
        internal int changesCount;
        internal StructuralChange[] changes;
        
        internal ArchetypeHash nextArchetypeHash;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Initialize() {
            this.currentArchetype = null;
            this.nextArchetypeHash = default;
            this.changes = new StructuralChange[16];
            this.changesCount = 0;
        }
    }
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal struct StructuralChange {
        private int value;
        
        internal TypeOffset typeOffset {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new TypeOffset(this.value >> 1);
        }
        
        internal bool isAddition {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (this.value & 1) == 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static StructuralChange Create(TypeOffset typeOffset, bool isAddition) {
            return new StructuralChange {
                value = (typeOffset.GetValue() << 1) | (isAddition ? 1 : 0)
            };
        }
    }
}