namespace SourceGenerators.Utils.Semantic {
    using System.Text;
    using Microsoft.CodeAnalysis;

    public static class GenericsSemantic {
        public static StringBuilder AppendGenericParams(this StringBuilder sb, INamedTypeSymbol typeSymbol) {
            if (typeSymbol.TypeArguments.Length == 0) {
                return sb;
            }
            
            sb.Append("<");
            
            for (int i = 0, length = typeSymbol.TypeArguments.Length; i < length; i++) {
                sb.Append(typeSymbol.TypeArguments[i].Name);
                if (i < length - 1) {
                    sb.Append(", ");
                }
            }
            
            sb.Append(">");

            return sb;
        }
    }
}