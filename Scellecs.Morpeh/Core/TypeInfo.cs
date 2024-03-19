namespace Scellecs.Morpeh {
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal readonly struct TypeInfo {
        internal readonly TypeOffset offset;
        internal readonly TypeId id;
        
        internal TypeInfo(TypeOffset offset, TypeId id) {
            this.offset = offset;
            this.id = id;
        }
        
        public override string ToString() {
            return $"TypeInfo({this.offset}, {this.id})";
        }

        public override int GetHashCode() {
            return this.offset.GetValue();
        }
        
        public static bool operator ==(TypeInfo a, TypeInfo b) {
            return a.offset.Equals(b.offset);
        }
        
        public static bool operator !=(TypeInfo a, TypeInfo b) {
            return !a.offset.Equals(b.offset);
        }
        
        public override bool Equals(object obj) {
            return obj is TypeInfo other && this.offset.Equals(other.offset);
        }
    }
}