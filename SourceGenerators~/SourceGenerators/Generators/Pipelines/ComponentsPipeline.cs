namespace SourceGenerators.Generators.Pipelines {
    using System.Linq;
    using System.Threading;
    using Components;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Unity;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    [Generator]
    public class ComponentsPipeline : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            // TODO: Combine with mono providers as a second stage
            var components = context.SyntaxProvider.ForAttributeWithMetadataName(
                    MorpehAttributes.COMPONENT_FULL_NAME,
                    static (s, _) => s is StructDeclarationSyntax,
                    static (s, ct) => ExtractComponentsToGenerate(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS);
            
            var providers = context.SyntaxProvider.ForAttributeWithMetadataName(
                    MorpehAttributes.MONO_PROVIDER_FULL_NAME,
                    predicate: static (s, _) => s is ClassDeclarationSyntax cds && cds.Parent is not TypeDeclarationSyntax,
                    transform: static (ctx, ct) => ExtractProvidersToGenerate(ctx, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .Where(candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS);

            context.RegisterSourceOutput(components, static (spc, component) => ComponentSourceGenerator.Generate(spc, component));
            context.RegisterSourceOutput(providers, static (spc, provider) => MonoProviderSourceGenerator.Generate(spc, provider));
        }

        private static ComponentToGenerate? ExtractComponentsToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
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
        
        private static ProviderToGenerate? ExtractProvidersToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            
            if (ctx.TargetNode is not ClassDeclarationSyntax syntaxNode) {
                return null;
            }
            
            if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                return null;
            }
            
            var               attribute        = ctx.Attributes.First();
            INamedTypeSymbol? monoProviderType = null;
            
            if (attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0] is { Kind: TypedConstantKind.Type, Value: INamedTypeSymbol symbol }) {
                monoProviderType = symbol;
            }
                
            if (monoProviderType is null) {
                return null;
            }
            
            return new ProviderToGenerate(
                TypeName: syntaxNode.Identifier.ToString(),
                TypeNamespace: typeSymbol.ContainingNamespace.IsGlobalNamespace ? null : typeSymbol.ContainingNamespace.ToDisplayString(),
                GenericParams: syntaxNode.TypeParameterList?.ToString() ?? string.Empty,
                GenericConstraints: typeSymbol.IsGenericType ? StringBuilderPool.Get().AppendGenericConstraints(typeSymbol).ToStringAndReturn() : string.Empty,
                ProviderTypeFullName: monoProviderType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                ProviderTypeVisibility: monoProviderType.DeclaredAccessibility,
                StashVariation: MorpehComponentHelpersSemantic.GetStashVariation(monoProviderType),
                Visibility: typeSymbol.DeclaredAccessibility);
        }
    }
}