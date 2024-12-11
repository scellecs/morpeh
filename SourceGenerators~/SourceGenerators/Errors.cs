namespace SourceGenerators {
    using Microsoft.CodeAnalysis;

    public static class Errors {
        private static readonly DiagnosticDescriptor NAMESPACE_MISSING = new(
            "MORPEH_ERR_001",
            "Component {0} is missing namespace",
            "Component {0} is missing namespace",
            "Morpeh",
            DiagnosticSeverity.Error,
            true,
            "Component {0} is missing namespace.");
        
        public static void ReportMissingNamespace(SourceProductionContext ctx, Location location, string componentName) {
            ctx.ReportDiagnostic(Diagnostic.Create(NAMESPACE_MISSING, location, componentName));
        }
    }
}