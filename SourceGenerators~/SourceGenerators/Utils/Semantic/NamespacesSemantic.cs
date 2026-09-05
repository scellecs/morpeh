namespace SourceGenerators.Utils.Semantic {
    using Microsoft.CodeAnalysis;

    public static class NamespacesSemantic {
        public static string? GetNamespaceString(this ISymbol symbol) {
            return symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToDisplayString();
        }
    }
}