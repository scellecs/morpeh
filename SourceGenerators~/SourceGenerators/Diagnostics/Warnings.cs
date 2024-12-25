namespace SourceGenerators.Diagnostics {
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class Warnings {
        // MORPEH_WRN_001
        
        public static readonly DiagnosticDescriptor INITIALIZER_WITH_ONUPDATE = new(
            "MORPEH_WRN_001",
            "The Initializer has an OnUpdate(float deltaTime) method. Did you mean to make a system instead?",
            "The Initializer has an OnUpdate(float deltaTime) method. Did you mean to make a system instead?",
            "Morpeh",
            DiagnosticSeverity.Warning,
            true,
            "The Initializer has an OnUpdate(float deltaTime) method. Did you mean to make a system instead?");
        
        public static void ReportInitializerWithOnUpdate(SourceProductionContext ctx, MethodDeclarationSyntax methodDeclaration) {
            ctx.ReportDiagnostic(Diagnostic.Create(INITIALIZER_WITH_ONUPDATE, methodDeclaration.GetLocation()));
        }
    }
}