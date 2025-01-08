namespace SourceGenerators.Utils.Semantic {
    using System.Text;
    using Microsoft.CodeAnalysis;

    public static class TypesSemantic {
        public static StringBuilder AppendVisibility(this StringBuilder sb, INamedTypeSymbol typeSymbol) {
            if (typeSymbol.DeclaredAccessibility == Accessibility.Public) {
                sb.Append("public");
            } else if (typeSymbol.DeclaredAccessibility == Accessibility.Internal) {
                sb.Append("internal");
            } else if (typeSymbol.DeclaredAccessibility == Accessibility.Protected) {
                sb.Append("protected");
            } else if (typeSymbol.DeclaredAccessibility == Accessibility.Private) {
                sb.Append("private");
            }
            
            return sb;
        }

        public static string GetFullyQualifiedNameHash(this ITypeSymbol typeSymbol) => GetFullyQualifiedNameHash(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        public static string GetFullyQualifiedNameHash(string fullyQualifiedName) => fullyQualifiedName.GetHashCode().ToString("X");

    }
}