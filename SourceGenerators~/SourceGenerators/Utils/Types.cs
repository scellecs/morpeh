namespace SourceGenerators.Utils {
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class Types {
        public static bool IsDeclaredInsideAnotherType(this TypeDeclarationSyntax type) => type.Parent is TypeDeclarationSyntax;
    }
}