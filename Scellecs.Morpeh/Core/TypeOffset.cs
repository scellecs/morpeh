namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct TypeOffset {
        private readonly int value;
        
        public TypeOffset(int value) {
            this.value = value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetValue() {
            return this.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(TypeOffset a, TypeOffset b) {
            return a.value == b.value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TypeOffset a, TypeOffset b) {
            return a.value != b.value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() {
            return $"TypeOffset({value})";
        }
    }
}