namespace Scellecs.Morpeh {
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal struct TypeInfo {
        internal TypeOffset offset;
        internal TypeId id;
        
        internal TypeInfo(TypeOffset offset, TypeId id) {
            this.offset = offset;
            this.id = id;
        }
        
        public override string ToString() {
            return $"TypeInfo({this.offset}, {this.id})";
        }
    }
}