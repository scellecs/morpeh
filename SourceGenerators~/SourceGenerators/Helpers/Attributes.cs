namespace SourceGenerators.Helpers {
    using System.Text;

    public static class Attributes {
        public static StringBuilder AppendIl2CppAttributes(this StringBuilder sb, int indent = 0) {
            sb.AppendIndent(indent).AppendLine("[Il2CppSetOption(Option.NullChecks, false)]");
            sb.AppendIndent(indent).AppendLine("[Il2CppSetOption(Option.ArrayBoundsChecks, false)]");
            sb.AppendIndent(indent).AppendLine("[Il2CppSetOption(Option.DivideByZeroChecks, false)]");
            
            return sb;
        }
        
        public static StringBuilder AppendConditionalAttribute(this StringBuilder sb, string conditional, int indent = 0) {
            sb.AppendIndent(indent).AppendLine($"[System.Diagnostics.Conditional(\"{conditional}\")]");
            
            return sb;
        }
    }
}