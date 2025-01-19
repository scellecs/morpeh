namespace SourceGenerators.Generators.Pipelines {
    using System;
    using System.Linq;
    using System.Threading;
    using Components;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Options;
    using Unity;
    using Utils.Logging;
    using Utils.NonSemantic;
    using Utils.Semantic;

    [Generator]
    public class ComponentsPipeline : IIncrementalGenerator {
        private const string PIPELINE_NAME = nameof(ComponentsPipeline);
        
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var options = context.ParseOptionsProvider
                .Select(static (parseOptions, _) => PreprocessorOptionsData.FromParseOptions(parseOptions));
            
            var components = context.SyntaxProvider.ForAttributeWithMetadataName(
                    MorpehAttributes.COMPONENT_FULL_NAME,
                    predicate: static (s, _) => s is StructDeclarationSyntax,
                    transform: static (s, ct) => ExtractComponentsToGenerate(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(PIPELINE_NAME, "components_ExtractComponentsToGenerate")
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS)
                .WithLogging(PIPELINE_NAME, "components_RemoveNullPass");
            
            var providers = context.SyntaxProvider.ForAttributeWithMetadataName(
                    MorpehAttributes.MONO_PROVIDER_FULL_NAME,
                    predicate: static (s, _) => s is ClassDeclarationSyntax,
                    transform: static (ctx, ct) => ExtractProvidersToGenerate(ctx, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(PIPELINE_NAME, "providers_ExtractProvidersToGenerate")
                .Where(candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS)
                .WithLogging(PIPELINE_NAME, "providers_RemoveNullPass");

            var providersOnlyInUnity = providers.Combine(options)
                .Where(static pair => pair.Right.IsUnity);

            context.RegisterSourceOutput(components.Combine(options), static (spc, pair) => ComponentSourceGenerator.Generate(spc, pair.Left, pair.Right));
            context.RegisterSourceOutput(providersOnlyInUnity, static (spc, pair) => MonoProviderSourceGenerator.Generate(spc, pair.Left, pair.Right));
        }

        private static ComponentToGenerate? ExtractComponentsToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            const string generatorStepName = nameof(ExtractComponentsToGenerate);
            
            ct.ThrowIfCancellationRequested();

            try {
                if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                    return null;
                }
            
                Logger.Log(PIPELINE_NAME, generatorStepName, $"Transform: {typeSymbol.Name}");
            
                var initialCapacity = 16;
                
                var args = ctx.Attributes[0].ConstructorArguments;
                if (args.Length >= 1 && args[0].Value is int capacity) {
                    initialCapacity = capacity;
                }
                
                var (genericParams, genericConstraints) = GenericsSemantic.GetGenericParamsAndConstraints(typeSymbol);
            
                return new ComponentToGenerate(
                    Hierarchy: ParentType.FromTypeSymbol(typeSymbol), 
                    TypeName: typeSymbol.Name,
                    TypeNamespace: typeSymbol.GetNamespaceString(),
                    GenericParams: genericParams,
                    GenericConstraints: genericConstraints,
                    InitialCapacity: initialCapacity,
                    StashVariation: MorpehComponentHelpersSemantic.GetStashVariation(typeSymbol),
                    Visibility: typeSymbol.DeclaredAccessibility);
            } catch (Exception e) {
                Logger.LogException(PIPELINE_NAME, generatorStepName, e);
                return null;
            }
        }
        
        private static ProviderToGenerate? ExtractProvidersToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            const string generatorStepName = nameof(ExtractProvidersToGenerate);
            
            ct.ThrowIfCancellationRequested();

            try {
                if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                    return null;
                }
            
                Logger.Log(PIPELINE_NAME, generatorStepName, $"Transform: {typeSymbol.Name}");
            
                INamedTypeSymbol? monoProviderType = null;
            
                var args = ctx.Attributes[0].ConstructorArguments;
            
                if (args.Length > 0 && args[0].Value is INamedTypeSymbol symbol) {
                    monoProviderType = symbol;
                }
                
                if (monoProviderType is null) {
                    return null;
                }
            
                var (genericParams, genericConstraints) = GenericsSemantic.GetGenericParamsAndConstraints(typeSymbol);
            
                return new ProviderToGenerate(
                    Hierarchy: ParentType.FromTypeSymbol(typeSymbol),
                    TypeName: typeSymbol.Name,
                    TypeNamespace: typeSymbol.GetNamespaceString(),
                    GenericParams: genericParams,
                    GenericConstraints: genericConstraints,
                    ProviderTypeFullName: monoProviderType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    ProviderTypeVisibility: monoProviderType.DeclaredAccessibility,
                    StashVariation: MorpehComponentHelpersSemantic.GetStashVariation(monoProviderType),
                    Visibility: typeSymbol.DeclaredAccessibility);
            } catch (Exception e) {
                Logger.LogException(PIPELINE_NAME, generatorStepName, e);
                return null;
            }
        }
    }
}