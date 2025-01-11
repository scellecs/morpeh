namespace SourceGenerators.Generators.Pipelines {
    using System.Linq;
    using System.Threading;
    using Components;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    public class ComponentsPipeline : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var components = context.SyntaxProvider.ForAttributeWithMetadataName(
                    MorpehAttributes.COMPONENT_FULL_NAME,
                    static (s, _) => s is StructDeclarationSyntax,
                    static (s, ct) => ExtractTypesToGenerate(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS);

            context.RegisterSourceOutput(components, static (spc, ct) => ComponentSourceGenerator.Generate(spc, ct));
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
            var typeNamespace       = containingNamespace.IsGlobalNamespace ? null : containingNamespace.ToDisplayString();

            string genericParams;
            string genericConstraints;

            if (typeSymbol.TypeParameters.Length > 0) {
                genericParams      = StringBuilderPool.Get().AppendGenericParams(syntaxNode).ToStringAndReturn();
                genericConstraints = StringBuilderPool.Get().AppendGenericConstraints(typeSymbol).ToStringAndReturn();
            }
            else {
                genericParams      = string.Empty;
                genericConstraints = string.Empty;
            }

            return new ComponentToGenerate(
                syntaxNode.Identifier.ToString(),
                typeNamespace,
                genericParams,
                genericConstraints,
                GetInitialCapacity(ctx.Attributes.First()),
                MorpehComponentHelpersSemantic.GetStashVariation(typeSymbol),
                Types.GetVisibilityModifier(syntaxNode));
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