namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct ArchetypeHash : IEquatable<ArchetypeHash> {
        private long value;
        
        public ArchetypeHash(long value) {
            this.value = value;
        }
        
        public ArchetypeHash(TypeHash typeHash) {
            this.value = typeHash.GetValue();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetValue() {
            return this.value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeHash Combine(ArchetypeHash otherArchetype) {
            ArchetypeHash hash;
            hash.value = this.value ^ otherArchetype.value;
            return hash;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArchetypeHash Combine(TypeHash typeHash) {
            ArchetypeHash hash;
            hash.value = this.value ^ typeHash.GetValue();
            return hash;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ArchetypeHash other) {
            return this.value == other.value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) {
            return obj is ArchetypeHash other && this.Equals(other);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ArchetypeHash a, ArchetypeHash b) {
            return a.value == b.value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ArchetypeHash a, ArchetypeHash b) {
            return a.value != b.value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() {
            return this.value.GetHashCode();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() {
            return $"ArchetypeId({this.value})";
        }
    }
}