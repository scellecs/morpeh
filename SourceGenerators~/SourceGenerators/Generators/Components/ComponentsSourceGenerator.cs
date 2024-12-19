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
    public class ComponentsSourceGenerator : IIncrementalGenerator {
        private const string ATTRIBUTE_FULL_NAME = "Scellecs.Morpeh.ComponentAttribute";
        
        private const string STASH_INITIAL_CAPACITY_ATTRIBUTE_NAME = "StashInitialCapacity";
        private const int DEFAULT_STASH_CAPACITY = 16;

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var structs = context.SyntaxProvider.ForAttributeWithMetadataName(
                ATTRIBUTE_FULL_NAME,
                (s, _) => s is StructDeclarationSyntax,
                (ctx, _) => (ctx.TargetNode as StructDeclarationSyntax, ctx.TargetSymbol as ITypeSymbol, ctx.SemanticModel));

            context.RegisterSourceOutput(structs, static (spc, pair) => {
                var (structDeclaration, typeSymbol, semanticModel) = pair;
                
                if (structDeclaration is null || typeSymbol is null) {
                    return;
                }
                
                if (structDeclaration.IsDeclaredInsideAnotherType()) {
                    Errors.ReportNestedDeclaration(spc, structDeclaration);
                    return;
                }

                var typeName = structDeclaration.Identifier.ToString();
                
                string genericParams;
                using (var scoped = StringBuilderPool.GetScoped()) {
                    genericParams = scoped.StringBuilder.AppendGenericParams(structDeclaration).ToString();
                }
                
                string genericConstraints;
                using (var scoped = StringBuilderPool.GetScoped()) {
                    genericConstraints = scoped.StringBuilder.AppendGenericConstraints(structDeclaration).ToString();
                }
                
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
                
                var specialization = MorpehComponentHelpersSemantic.GetStashSpecialization(semanticModel, typeSymbol);
            
                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
            
                sb.AppendIndent(indent).AppendLine("using Scellecs.Morpeh;");
                
                sb.AppendBeginNamespace(structDeclaration, indent).AppendLine();
                
                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent).AppendVisibility(structDeclaration)
                    .Append(" partial struct ")
                    .Append(typeName)
                    .Append(genericParams)
                    .Append(" : ")
                    .Append(specialization.constraintInterface)
                    .Append(' ')
                    .Append(genericConstraints)
                    .AppendLine(" {");
                
                using (indent.Scope()) {
                    sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                    sb.AppendIndent(indent).Append("public static ").Append(specialization.type).Append(" GetStash(World world) => world.").Append(specialization.getStashMethod)
                        .Append("(")
                        .Append("capacity: ").Append(stashInitialCapacity)
                        .AppendLine(");");
                }
                sb.AppendIndent(indent).AppendLine("}");
                
                sb.AppendEndNamespace(structDeclaration, indent).AppendLine();
                
                spc.AddSource($"{structDeclaration.Identifier.Text}.component_{structDeclaration.GetStableFileCompliantHash()}.g.cs", sb.ToString());
                
                StringBuilderPool.Return(sb);
                IndentSourcePool.Return(indent);
            });
        }
    }
}