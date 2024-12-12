namespace SourceGenerators {
    using System;
    using System.Linq;
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
                
                if (structDeclaration.IsDeclaredInsideAnotherType()) {
                    Errors.ReportNestedDeclaration(spc, structDeclaration);
                    return;
                }

                var typeName           = structDeclaration.Identifier.ToString();
                var genericParams      = new StringBuilder().AppendGenericParams(structDeclaration).ToString();
                var genericConstraints = new StringBuilder().AppendGenericConstraints(structDeclaration).ToString();

                var isTag = !structDeclaration.HasAnyDataField();
                var isDisposable = structDeclaration.BaseList?.Types.Any(t => t.Type.ToString().EndsWith("IDisposable")) ?? false;
            
                var sb = new StringBuilder();
            
                sb.AppendBeginNamespace(structDeclaration).AppendLine();
            
                // TODO: Choose correct Stash specialization based on the component information (fields count, types, etc.)
                sb.AppendLine("using Scellecs.Morpeh;");
                sb.AppendLine($"public static class {typeName}__Generated{genericParams} {genericConstraints} {{");

                if (isTag) {
                    if (isDisposable) {
                        Errors.ReportTagDisposable(spc, structDeclaration);
                        return;
                    }
                    
                    sb.AppendLine($"public static TagStash GetStash(World world) => world.GetTagStash<{typeName}{genericParams}>();");
                } else if (isDisposable) {
                    // TODO: Choose either unmanaged or managed stash based on the component information.
                    // TODO: Or we can just make all of them pinned with GetNativeHandle() method.
                    /*
                    if (typeSymbol.IsUnmanagedType) {
                        sb.AppendLine($"public static StashUD<{typeName}{genericParams}> GetStash(World world) => world.GetStashUD<{typeName}{genericParams}>();");
                    } else {
                        sb.AppendLine($"public static StashD<{typeName}{genericParams}> GetStash(World world) => world.GetStashD<{typeName}{genericParams}>();");
                    }
                    */
                    sb.AppendLine($"public static StashD<{typeName}{genericParams}> GetStash(World world) => world.GetStashD<{typeName}{genericParams}>();");
                } else {
                    // TODO: Choose either unmanaged or managed stash based on the component information.
                    /*
                    if (typeSymbol.IsUnmanagedType) {
                        sb.AppendLine($"public static StashU<{typeName}{genericParams}> GetStash(World world) => world.GetStashUD<{typeName}{genericParams}>();");
                    } else {
                        sb.AppendLine($"public static Stash<{typeName}{genericParams}> GetStash(World world) => world.GetStashD<{typeName}{genericParams}>();");
                    }
                    */
                    sb.AppendLine($"public static Stash<{typeName}{genericParams}> GetStash(World world) => world.GetStash<{typeName}{genericParams}>();");
                }

                sb.AppendLine("}");
                sb.AppendEndNamespace(structDeclaration).AppendLine();
                
                spc.AddSource($"{structDeclaration.Identifier.Text}.component_extensions.g.cs", sb.ToString());
            });
        }
    }
}