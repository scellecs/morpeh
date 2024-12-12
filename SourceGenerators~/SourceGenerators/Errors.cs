namespace SourceGenerators {
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class Errors {
        // MORPEH_ERR_001
        
        private static readonly DiagnosticDescriptor NESTED_DECLARATION = new(
            "MORPEH_ERR_001",
            "Component {0} is declared inside another type.",
            "Component {0} is declared inside another type.",
            "Morpeh",
            DiagnosticSeverity.Error,
            true,
            "Component {0} is declared inside another type.");
        
        public static void ReportNestedDeclaration(SourceProductionContext ctx, TypeDeclarationSyntax typeDeclarationSyntax) {
            ctx.ReportDiagnostic(Diagnostic.Create(NESTED_DECLARATION, typeDeclarationSyntax.GetLocation(), typeDeclarationSyntax.Identifier.Text));
        }
        
        // MORPEH_ERR_002
        
        private static readonly DiagnosticDescriptor TAG_DISPOSABLE = new(
            "MORPEH_ERR_002",
            "Component {0} is both disposable and a tag.",
            "Component {0} is both disposable and a tag.",
            "Morpeh",
            DiagnosticSeverity.Error,
            true,
            "Component {0} is both disposable and a tag.");

        public static void ReportTagDisposable(SourceProductionContext ctx, TypeDeclarationSyntax typeDeclarationSyntax) {
            ctx.ReportDiagnostic(Diagnostic.Create(TAG_DISPOSABLE, typeDeclarationSyntax.GetLocation(), typeDeclarationSyntax.Identifier.Text));
        }
    }
}