namespace SourceGenerators {
    using System;
    using System.Linq;
    using System.Text;
    using Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Utils;

    [Generator]
    public class ComponentsExtensionsSourceGenerator : IIncrementalGenerator {
        private const string COMPONENT_INTERFACE_NAME = "Scellecs.Morpeh.IComponent";

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var structs = context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (syntaxNode, _) => syntaxNode is StructDeclarationSyntax,
                    static (ctx, _) => (declaration: (StructDeclarationSyntax)ctx.Node, model: ctx.SemanticModel))
                .Where(static pair => pair.declaration is not null);

            var componentInterface = context.CompilationProvider.Select(static (compilation, _) => compilation.GetTypeByMetadataName(COMPONENT_INTERFACE_NAME));

            context.RegisterSourceOutput(structs.Combine(componentInterface), static (spc, pair) => {
                var ((structDeclaration, semanticModel), iComponent) = pair;
                
                if (iComponent is null) {
                    return;
                }

                if (semanticModel.GetDeclaredSymbol(structDeclaration) is not ITypeSymbol structSymbol || !structSymbol.AllInterfaces.Contains(iComponent)) {
                    return;
                }
                
                if (structDeclaration.IsDeclaredInsideAnotherType()) {
                    Errors.ReportNestedDeclaration(spc, structDeclaration);
                    return;
                }

                var typeName           = structDeclaration.Identifier.ToString();
                var typeHash     = structDeclaration.GetStableFileCompliantHash();
                var genericParams      = new StringBuilder().AppendGenericParams(structDeclaration).ToString();
                var genericConstraints = new StringBuilder().AppendGenericConstraints(structDeclaration).ToString();

                var specialization = ComponentHelpers.GetStashSpecialization(structDeclaration);

                if (specialization is { isTag: true, isDisposable: true }) {
                    Errors.ReportTagDisposable(spc, structDeclaration);
                    return;
                }
            
                var sb = new StringBuilder();
            
                sb.AppendBeginNamespace(structDeclaration).AppendLine();
            
                sb.AppendLine("using Scellecs.Morpeh;");
                sb.AppendLine($"public static class {typeName}__Metadata{genericParams} {genericConstraints} {{");
                sb.Append(' ', 2).AppendLine($"public static {specialization.type} GetStash(World world) => world.{specialization.getStashMethod}();");
                sb.AppendLine("}");
                sb.AppendEndNamespace(structDeclaration).AppendLine();
                
                spc.AddSource($"{structDeclaration.Identifier.Text}.component_extensions_{typeHash}.g.cs", sb.ToString());
            });
        }
    }
}