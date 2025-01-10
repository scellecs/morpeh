namespace SourceGenerators.Generators.Components {
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Utils.NonSemantic;
    using Utils.Semantic;
    using Utils.Pools;

    [Generator]
    public class ComponentSourceGenerator : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var components = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.COMPONENT_FULL_NAME,
                predicate: static (s, _) => s is StructDeclarationSyntax,
                transform: static (s, ct) => ExtractTypesToGenerate(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS);

            context.RegisterSourceOutput(components, static (spc, component) => {
                var fullTypeName = StringBuilderPool.Get().Append(component.typeName).Append(component.genericParams).ToStringAndReturn();
                var specialization = MorpehComponentHelpersSemantic.GetStashSpecialization(component.stashVariation, fullTypeName);
            
                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                if (component.typeNamespace != null) {
                    sb.AppendIndent(indent).Append("namespace ").Append(component.typeNamespace).AppendLine(" {");
                    indent.Right();
                }
                
                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent)
                    .Append(component.visibility)
                    .Append(" partial struct ")
                    .Append(component.typeName)
                    .Append(component.genericParams)
                    .Append(" : ")
                    .Append(specialization.constraintInterface)
                    .Append(' ')
                    .Append(component.genericConstraints)
                    .AppendLine(" {");
                
                using (indent.Scope()) {
                    sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                    sb.AppendIndent(indent).Append("public static ")
                        .Append(specialization.type)
                        .Append(" GetStash(Scellecs.Morpeh.World world) => Scellecs.Morpeh.WorldStashExtensions.")
                        .Append(specialization.getStashMethod)
                        .Append("(world, capacity: ")
                        .Append(component.initialCapacity)
                        .AppendLine(");");
                }
                sb.AppendIndent(indent).AppendLine("}");

                if (component.typeNamespace != null) {
                    indent.Left();
                    sb.AppendIndent(indent).AppendLine("}");
                }
                
                // TODO: Think of a better way to handle collisions between names.
                spc.AddSource($"{component.typeName}.component_{Guid.NewGuid():N}.g.cs", sb.ToStringAndReturn());
                
                IndentSourcePool.Return(indent);
            });
        }
        
        private static ComponentToGenerate? ExtractTypesToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            
            if (ctx.TargetNode is not StructDeclarationSyntax syntaxNode || syntaxNode.Parent is TypeDeclarationSyntax) {
                return null;
            }
            
            if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                return null;
            }

            var containingNamespace = typeSymbol.ContainingNamespace;
            var typeNamespace = containingNamespace.IsGlobalNamespace ? null : containingNamespace.ToDisplayString();

            string genericParams;
            string genericConstraints;
            
            if (typeSymbol.TypeParameters.Length > 0) {
                genericParams      = StringBuilderPool.Get().AppendGenericParams(syntaxNode).ToStringAndReturn();
                genericConstraints = StringBuilderPool.Get().AppendGenericConstraints(typeSymbol).ToStringAndReturn();
            } else {
                genericParams      = string.Empty;
                genericConstraints = string.Empty;
            }
            
            return new ComponentToGenerate(
                typeName: syntaxNode.Identifier.ToString(),
                typeNamespace: typeNamespace,
                genericParams: genericParams,
                genericConstraints: genericConstraints,
                visibility: Types.GetVisibility(syntaxNode),
                initialCapacity: GetInitialCapacity(ctx.Attributes.First()),
                stashVariation: MorpehComponentHelpersSemantic.GetStashVariation(typeSymbol));
        }
        
        private static int GetInitialCapacity(AttributeData attribute) {
            var initialCapacity = 16;
            
            var args = attribute.ConstructorArguments;
            if (args.Length >= 1 && args[0].Value is int capacity) {
                initialCapacity = capacity;
            }
            
            return initialCapacity;
        }
    }
}