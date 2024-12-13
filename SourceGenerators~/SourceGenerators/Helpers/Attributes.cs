namespace SourceGenerators.Helpers {
    using System.Runtime.CompilerServices;
    using System.Text;

    public static class Attributes {
        public static StringBuilder AppendIl2CppAttributes(this StringBuilder sb, IndentSource indent) {
            sb.AppendIndent(indent).AppendLine("[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]");
            sb.AppendIndent(indent).AppendLine("[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]");
            sb.AppendIndent(indent).AppendLine("[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]");
            
            return sb;
        }
        
        public static StringBuilder AppendConditionalAttribute(this StringBuilder sb, string conditional, IndentSource indent) {
            sb.AppendIndent(indent).AppendLine($"[System.Diagnostics.Conditional(\"{conditional}\")]");
            
            return sb;
        }
        
        public static StringBuilder AppendInlining(this StringBuilder sb, MethodImplOptions options, IndentSource indent) {
            sb.AppendIndent(indent).AppendLine($"[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.{options})]");
            
            return sb;
        }
    }
}