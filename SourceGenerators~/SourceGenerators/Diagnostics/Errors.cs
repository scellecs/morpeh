namespace SourceGenerators.Diagnostics {
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class Errors {
        // MORPEH_ERR_001
        
        public static readonly DiagnosticDescriptor NESTED_DECLARATION = new(
            "MORPEH_ERR_001",
            "{0} cannot be declared inside another type.",
            "{0} cannot be declared inside another type.",
            "Morpeh",
            DiagnosticSeverity.Error,
            true,
            "{0} cannot be declared inside another type.");
        
        public static void ReportNestedDeclaration(SourceProductionContext ctx, TypeDeclarationSyntax typeDeclaration) {
            ctx.ReportDiagnostic(Diagnostic.Create(NESTED_DECLARATION, typeDeclaration.GetLocation(), typeDeclaration.Identifier.Text));
        }
        
        // MORPEH_ERR_002
        
        private static readonly DiagnosticDescriptor MISSING_LOOP_TYPE = new(
            "MORPEH_ERR_002",
            "System does not have a [Loop] attribute.",
            "System does not have a [Loop] attribute.",
            "Morpeh",
            DiagnosticSeverity.Error,
            true,
            "System does not have a [Loop] attribute.");
        
        public static void ReportMissingLoopType(SourceProductionContext ctx, ISymbol symbol) {
            ctx.ReportDiagnostic(Diagnostic.Create(MISSING_LOOP_TYPE, symbol.Locations.First()));
        }
        
        // MORPEH_ERR_003
        
        private static readonly DiagnosticDescriptor LOOP_TYPE_NOT_SYSTEM_FIELD = new(
            "MORPEH_ERR_003",
            "The type is not a System so it cannot have a loop specified.",
            "The type is not a System so it cannot have a loop specified.",
            "Morpeh",
            DiagnosticSeverity.Error,
            true,
            "The type is not a System so it cannot have a loop specified.");

        public static void ReportLoopTypeOnNonSystemField(SourceProductionContext ctx, ISymbol symbol) {
            ctx.ReportDiagnostic(Diagnostic.Create(LOOP_TYPE_NOT_SYSTEM_FIELD, symbol.Locations.First()));
        }
        
        // MORPEH_ERR_004
        
        public static readonly DiagnosticDescriptor INVALID_INJECTION_SOURCE_TYPE = new(
            "MORPEH_ERR_004",
            "Cannot register {0} as it is not a class type.",
            "Cannot register {0} as it is not a class type.",
            "Morpeh",
            DiagnosticSeverity.Error,
            true,
            "Cannot register {0} as it is not a class type.");

        public static void ReportInvalidInjectionSourceType(SourceProductionContext ctx, ISymbol symbol, string typeName) {
            ctx.ReportDiagnostic(Diagnostic.Create(INVALID_INJECTION_SOURCE_TYPE, symbol.Locations.First(), typeName));
        }
        
        // MORPEH_ERR_005
        
        public static readonly DiagnosticDescriptor NO_PARAMETERLESS_CONSTRUCTOR_FOR_SERVICE = new(
            "MORPEH_ERR_005",
            "Cannot create an instance of {0} because it does not have a parameterless constructor.",
            "Cannot create an instance of {0} because it does not have a parameterless constructor.",
            "Morpeh",
            DiagnosticSeverity.Error,
            true,
            "Cannot create an instance of {0} because it does not have a parameterless constructor.");

        public static void ReportNoParameterlessConstructor(SourceProductionContext ctx, ISymbol symbol, string typeName) {
            ctx.ReportDiagnostic(Diagnostic.Create(NO_PARAMETERLESS_CONSTRUCTOR_FOR_SERVICE, symbol.Locations.First(), typeName));
        }
    }
}