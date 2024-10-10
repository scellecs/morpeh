namespace Scellecs.Morpeh {
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct TypeInfo {
        internal TypeHash hash;
        internal int id;
        internal int stashSize;
        
        internal TypeInfo(TypeHash hash, int id, int stashSize) {
            this.id = id;
            this.hash = hash;
            this.stashSize = stashSize;
        }
        
        public override string ToString() {
            return $"TypeInfo({this.hash}, {this.id})";
        }
    }
}