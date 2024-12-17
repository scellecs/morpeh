namespace SourceGenerators.Generators.ComponentsMetadata {
    using System.Runtime.CompilerServices;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Diagnostics;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Utils.NonSemantic;
    using Utils.Pools;

    [Generator]
    public class ComponentsMetadataSourceGenerator : IIncrementalGenerator {
        private const string COMPONENT_INTERFACE_NAME = "Scellecs.Morpeh.IComponent";
        private const string STASH_INITIAL_CAPACITY_ATTRIBUTE_NAME = "StashInitialCapacity";
        private const int DEFAULT_STASH_CAPACITY = 16;

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var structs = context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (syntaxNode, _) => syntaxNode is StructDeclarationSyntax,
                    static (ctx, _) => (declaration: (StructDeclarationSyntax)ctx.Node, model: ctx.SemanticModel))
                .Where(static pair => pair.declaration is not null);

            var componentInterface = context.CompilationProvider
                .Select(static (compilation, _) => compilation.GetTypeByMetadataName(COMPONENT_INTERFACE_NAME));

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

                var typeName             = structDeclaration.Identifier.ToString();
                var genericParams        = new StringBuilder().AppendGenericParams(structDeclaration).ToString();
                var genericConstraints   = new StringBuilder().AppendGenericConstraints(structDeclaration).ToString();
                
                var stashInitialCapacity = DEFAULT_STASH_CAPACITY;
                
                if (structDeclaration.AttributeLists.Count > 0) {
                    for (int i = 0, length = structDeclaration.AttributeLists.Count; i < length; i++) {
                        for (int j = 0, attributesCount = structDeclaration.AttributeLists[i].Attributes.Count; j < attributesCount; j++) {
                            var attribute = structDeclaration.AttributeLists[i].Attributes[j];
                            
                            if (attribute.Name.ToString() != STASH_INITIAL_CAPACITY_ATTRIBUTE_NAME) {
                                continue;
                            }

                            if (attribute.ArgumentList?.Arguments.Count == 0) {
                                continue;
                            }

                            var argument = attribute.ArgumentList?.Arguments[0];
                            if (argument?.Expression is not LiteralExpressionSyntax literalExpressionSyntax) {
                                continue;
                            }

                            if (int.TryParse(literalExpressionSyntax.Token.ValueText, out var value)) {
                                stashInitialCapacity = value;
                            }
                        }
                    }
                }
                

                var specialization = MorpehComponentHelpersSemantic.GetStashSpecialization(semanticModel, structDeclaration);
            
                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
            
                sb.AppendBeginNamespace(structDeclaration, indent).AppendLine();
            
                sb.AppendIndent(indent).AppendLine("using Scellecs.Morpeh;");

                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent).AppendLine($"public static class {typeName}__Metadata{genericParams} {genericConstraints} {{");
                using (indent.Scope()) {
                    sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                    sb.AppendIndent(indent).Append($"public static {specialization.type} GetStash(World world) => world.{specialization.getStashMethod}(")
                        .Append("capacity: ").Append(stashInitialCapacity)
                        .AppendLine(");");
                }
                sb.AppendIndent(indent).AppendLine("}");
                
                sb.AppendEndNamespace(structDeclaration, indent).AppendLine();
                
                spc.AddSource($"{structDeclaration.Identifier.Text}.component_extensions_{structDeclaration.GetStableFileCompliantHash()}.g.cs", sb.ToString());
                
                StringBuilderPool.Return(sb);
                IndentSourcePool.Return(indent);
            });
        }
    }
}