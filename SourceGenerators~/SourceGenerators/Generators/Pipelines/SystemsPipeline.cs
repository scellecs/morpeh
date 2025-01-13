namespace SourceGenerators.Generators.Pipelines {
    using System;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Systems;
    using Utils.Logging;
    using Utils.NonSemantic;
    using Utils.Semantic;

    [Generator]
    public class SystemsPipeline : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var systems = context.SyntaxProvider.ForAttributeWithMetadataName(
                    MorpehAttributes.SYSTEM_FULL_NAME,
                    predicate: static (s, _) => s is TypeDeclarationSyntax syntaxNode && syntaxNode.Parent is not TypeDeclarationSyntax,
                    transform: static (s, ct) => ExtractSystemsToGenerate(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(nameof(SystemsPipeline), "systems_ExtractSystemsToGenerate")
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS)
                .WithLogging(nameof(SystemsPipeline), "systems_RemoveNullPass");
            
            var initializers = context.SyntaxProvider.ForAttributeWithMetadataName(
                    MorpehAttributes.INITIALIZER_FULL_NAME,
                    predicate: static (s, _) => s is TypeDeclarationSyntax syntaxNode && syntaxNode.Parent is not TypeDeclarationSyntax,
                    transform: static (s, ct) => ExtractInitializersToGenerate(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(nameof(SystemsPipeline), "initializers_ExtractInitializersToGenerate")
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS)
                .WithLogging(nameof(SystemsPipeline), "initializers_RemoveNullPass");
            
            context.RegisterSourceOutput(systems, static (spc, system) => SystemSourceGenerator.Generate(spc, system));
            context.RegisterSourceOutput(initializers, static (spc, initializer) => InitializerSourceGenerator.Generate(spc, initializer));
        }
        
        private static SystemToGenerate? ExtractSystemsToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            const string generatorStepName = nameof(ExtractSystemsToGenerate);
            
            ct.ThrowIfCancellationRequested();

            try {
                if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                    return null;
                }

                var args = ctx.Attributes[0].ConstructorArguments;

                var skipCommit = false;
                if (args.Length >= 1 && args[0].Value is bool skipCommitValue) {
                    skipCommit = skipCommitValue;
                }

                var alwaysEnabled = false;
                if (args.Length >= 2 && args[1].Value is bool alwaysEnabledValue) {
                    alwaysEnabled = alwaysEnabledValue;
                }

                var (genericParams, genericConstraints) = GenericsSemantic.GetGenericParamsAndConstraints(typeSymbol);

                return new SystemToGenerate(
                    TypeName: typeSymbol.Name,
                    TypeNamespace: typeSymbol.GetNamespaceString(),
                    GenericParams: genericParams,
                    GenericConstraints: genericConstraints,
                    StashRequirements: MorpehComponentHelpersSemantic.GetStashRequirements(typeSymbol),
                    TypeKind: typeSymbol.TypeKind,
                    Visibility: typeSymbol.DeclaredAccessibility,
                    SkipCommit: skipCommit,
                    AlwaysEnabled: alwaysEnabled);
            } catch (Exception e) {
                Logger.LogException(nameof(SystemsPipeline), generatorStepName, e);
                return null;
            }
        }
        
        private static InitializerToGenerate? ExtractInitializersToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            const string generatorStepName = nameof(ExtractInitializersToGenerate);
            
            ct.ThrowIfCancellationRequested();

            try {
                if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                    return null;
                }

                var (genericParams, genericConstraints) = GenericsSemantic.GetGenericParamsAndConstraints(typeSymbol);

                return new InitializerToGenerate(
                    TypeName: typeSymbol.Name,
                    TypeNamespace: typeSymbol.GetNamespaceString(),
                    GenericParams: genericParams,
                    GenericConstraints: genericConstraints,
                    StashRequirements: MorpehComponentHelpersSemantic.GetStashRequirements(typeSymbol),
                    TypeKind: typeSymbol.TypeKind,
                    Visibility: typeSymbol.DeclaredAccessibility);
            } catch (Exception e) {
                Logger.LogException(nameof(SystemsPipeline), generatorStepName, e);
                return null;
            }
        }
    }
}