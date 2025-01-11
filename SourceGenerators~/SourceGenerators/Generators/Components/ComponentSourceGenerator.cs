namespace SourceGenerators.Generators.Components {
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Utils.NonSemantic;
    using Utils.Semantic;
    using Utils.Pools;

    [Generator]
    public class ComponentSourceGenerator : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            // TODO: Implement monoprovider generator as a separate stage in the pipeline.
            // TODO: This will allow to reuse the same information, as providers are always a subset of components.
            var components = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.COMPONENT_FULL_NAME,
                predicate: static (s, _) => s is StructDeclarationSyntax,
                transform: static (s, ct) => ExtractTypesToGenerate(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS);

            context.RegisterSourceOutput(components, static (spc, component) => {
                var fullTypeName = StringBuilderPool.Get().Append(component.TypeName).Append(component.GenericParams).ToStringAndReturn();
            
                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                if (component.TypeNamespace != null) {
                    sb.AppendIndent(indent).Append("namespace ").Append(component.TypeNamespace).AppendLine(" {");
                    indent.Right();
                }
                
                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent)
                    .Append(Types.GetVisibilityModifierString(component.Visibility))
                    .Append(" partial struct ")
                    .Append(component.TypeName)
                    .Append(component.GenericParams)
                    .Append(" : ")
                    .Append(MorpehComponentHelpersSemantic.GetStashSpecializationConstraintInterface(component.StashVariation))
                    .Append(' ')
                    .Append(component.GenericConstraints)
                    .AppendLine(" {");
                
                using (indent.Scope()) {
                    sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                    sb.AppendIndent(indent).Append("public static ")
                        .Append(MorpehComponentHelpersSemantic.GetStashSpecializationType(component.StashVariation, fullTypeName))
                        .Append(" GetStash(Scellecs.Morpeh.World world) => Scellecs.Morpeh.WorldStashExtensions.")
                        .Append(MorpehComponentHelpersSemantic.GetStashSpecializationGetStashMethod(component.StashVariation, fullTypeName))
                        .Append("(world, capacity: ")
                        .Append(component.InitialCapacity)
                        .AppendLine(");");
                }
                sb.AppendIndent(indent).AppendLine("}");

                if (component.TypeNamespace != null) {
                    indent.Left();
                    sb.AppendIndent(indent).AppendLine("}");
                }
                
                // TODO: Think of a better way to handle collisions between names.
                spc.AddSource($"{component.TypeName}.component_{Guid.NewGuid():N}.g.cs", sb.ToStringAndReturn());
                
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
                TypeName: syntaxNode.Identifier.ToString(),
                TypeNamespace: typeNamespace,
                GenericParams: genericParams,
                GenericConstraints: genericConstraints,
                InitialCapacity: GetInitialCapacity(ctx.Attributes.First()),
                StashVariation: MorpehComponentHelpersSemantic.GetStashVariation(typeSymbol),
                Visibility: Types.GetVisibilityModifier(syntaxNode));
        }
        
        private static int GetInitialCapacity(AttributeData attribute) {
            var initialCapacity = 16;
            
            var args = attribute.ConstructorArguments;
            if (args.Length >= 1 && args[0].Value is int capacity) {
                initialCapacity = capacity;
            }
            
            return initialCapacity;
        }
        
        private record struct ComponentToGenerate(
            string TypeName,
            string? TypeNamespace,
            string GenericParams,
            string GenericConstraints,
            int InitialCapacity,
            StashVariation StashVariation,
            SyntaxKind Visibility);
    }
}