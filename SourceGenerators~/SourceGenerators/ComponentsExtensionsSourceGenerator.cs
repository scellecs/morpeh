namespace SourceGenerators {
    using System.Text;
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

                var typeName = structDeclaration.Identifier.ToString();
                
                if (structDeclaration.IsDeclaredInsideAnotherType()) {
                    Errors.ReportNestedDeclaration(spc, structDeclaration);
                    return;
                }

                var genericParams      = new StringBuilder().AppendGenericParams(structDeclaration).ToString();
                var genericConstraints = new StringBuilder().AppendGenericConstraints(structDeclaration).ToString();
            
                var sb = new StringBuilder();
            
                sb.AppendBeginNamespace(structDeclaration).AppendLine();
            
                // TODO: Choose correct Stash specialization based on the component information (fields count, types, etc.)
                sb.AppendLine(@$"using Scellecs.Morpeh;
public static class {typeName}__Generated{genericParams} {genericConstraints} {{
    public static Stash<{typeName}{genericParams}> GetStash(World world) => world.GetStash<{typeName}{genericParams}>();
}}");
                sb.AppendEndNamespace(structDeclaration).AppendLine();
                
                spc.AddSource($"{structDeclaration.Identifier.Text}.component_extensions.g.cs", sb.ToString());
            });
        }
    }
}