namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct TypeId {
        private readonly long value;
        
        public TypeId(long value) {
            this.value = value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetValue() {
            return this.value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeId Combine(TypeId other) {
            return new TypeId(this.value ^ other.value);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() {
            return $"TypeId({value})";
        }
    }
}