namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct TypeId : IEquatable<TypeId> {
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
        public bool Equals(TypeId other) {
            return value == other.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) {
            return obj is TypeId other && Equals(other);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(TypeId a, TypeId b) {
            return a.value == b.value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(TypeId a, TypeId b) {
            return a.value != b.value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() {
            return value.GetHashCode();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() {
            return $"TypeId({value})";
        }
    }
}