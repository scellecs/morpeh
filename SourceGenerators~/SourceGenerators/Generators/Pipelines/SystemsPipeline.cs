namespace SourceGenerators.Generators.Pipelines {
    using System;
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Options;
    using Systems;
    using Utils.Collections;
    using Utils.Logging;
    using Utils.NonSemantic;
    using Utils.Semantic;

    [Generator]
    public class SystemsPipeline : IIncrementalGenerator {
        private const string PIPELINE_NAME = nameof(SystemsPipeline);
        
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var options = context.ParseOptionsProvider
                .Select(static (parseOptions, _) => PreprocessorOptionsData.FromParseOptions(parseOptions));

            var updateMiddlewares = context.SyntaxProvider.ForAttributeWithMetadataName(
                    MorpehAttributes.SYSTEM_UPDATE_MIDDLEWARE_FULL_NAME,
                    predicate: static (s, _) => s is TypeDeclarationSyntax,
                    transform: static (s, ct) => ExtractSystemUpdateMiddlewares(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(PIPELINE_NAME, "systemsUpdateMiddlewares_ExtractSystemUpdateMiddlewares")
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(PIPELINE_NAME, "systemsUpdateMiddlewares_RemoveNullPass")
                .Collect()
                .Select(static (middlewares, ct) => SortSystemUpdateMiddlewares(middlewares, ct));
            
            var systems = context.SyntaxProvider.ForAttributeWithMetadataName(
                    MorpehAttributes.SYSTEM_FULL_NAME,
                    predicate: static (s, _) => s is TypeDeclarationSyntax,
                    transform: static (s, ct) => ExtractSystemsToGenerate(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(PIPELINE_NAME, "systems_ExtractSystemsToGenerate")
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS)
                .WithLogging(PIPELINE_NAME, "systems_RemoveNullPass")
                .Combine(options)
                .Combine(updateMiddlewares);
            
            var initializers = context.SyntaxProvider.ForAttributeWithMetadataName(
                    MorpehAttributes.INITIALIZER_FULL_NAME,
                    predicate: static (s, _) => s is TypeDeclarationSyntax,
                    transform: static (s, ct) => ExtractInitializersToGenerate(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(PIPELINE_NAME, "initializers_ExtractInitializersToGenerate")
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS)
                .WithLogging(PIPELINE_NAME, "initializers_RemoveNullPass")
                .Combine(options);
            
            context.RegisterSourceOutput(systems, static (spc, pair) => SystemSourceGenerator.Generate(spc, pair.Left.Left, pair.Right, pair.Left.Right));
            context.RegisterSourceOutput(initializers, static (spc, pair) => InitializerSourceGenerator.Generate(spc, pair.Left, pair.Right));
        }
        
        private static SystemToGenerate? ExtractSystemsToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            const string generatorStepName = nameof(ExtractSystemsToGenerate);
            
            ct.ThrowIfCancellationRequested();

            try {
                if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                    return null;
                }
                
                Logger.Log(PIPELINE_NAME, generatorStepName, $"Transform: {typeSymbol.Name}");

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
                    Hierarchy: ParentType.FromTypeSymbol(typeSymbol), 
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
                Logger.LogException(PIPELINE_NAME, generatorStepName, e);
                return null;
            }
        }
        
        private static EquatableArray<SystemUpdateMiddleware> SortSystemUpdateMiddlewares(ImmutableArray<SystemUpdateMiddleware> middlewares, CancellationToken ct) {
            const string generatorStepName = nameof(SortSystemUpdateMiddlewares);
            
            ct.ThrowIfCancellationRequested();

            try {
                var array = new EquatableArray<SystemUpdateMiddleware>(middlewares);
                array.Sort(static (a, b) => a.Priority.CompareTo(b.Priority));
                return array;
            } catch (Exception e) {
                Logger.LogException(PIPELINE_NAME, generatorStepName, e);
                return new EquatableArray<SystemUpdateMiddleware>(Array.Empty<SystemUpdateMiddleware>());
            }
        }
        
        private static SystemUpdateMiddleware? ExtractSystemUpdateMiddlewares(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            const string generatorStepName = nameof(ExtractSystemUpdateMiddlewares);
            
            ct.ThrowIfCancellationRequested();

            try {
                if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                    return null;
                }
                
                Logger.Log(PIPELINE_NAME, generatorStepName, $"Transform: {typeSymbol.Name}");

                var priority = 0;
                
                var args = ctx.Attributes[0].ConstructorArguments;
                if (args.Length >= 1 && args[0].Value is int priorityValue) {
                    priority = priorityValue;
                }
                
                return new SystemUpdateMiddleware(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), priority);
            } catch (Exception e) {
                Logger.LogException(PIPELINE_NAME, generatorStepName, e);
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
                
                Logger.Log(PIPELINE_NAME, generatorStepName, $"Transform: {typeSymbol.Name}");

                var (genericParams, genericConstraints) = GenericsSemantic.GetGenericParamsAndConstraints(typeSymbol);

                return new InitializerToGenerate(
                    Hierarchy: ParentType.FromTypeSymbol(typeSymbol), 
                    TypeName: typeSymbol.Name,
                    TypeNamespace: typeSymbol.GetNamespaceString(),
                    GenericParams: genericParams,
                    GenericConstraints: genericConstraints,
                    StashRequirements: MorpehComponentHelpersSemantic.GetStashRequirements(typeSymbol),
                    TypeKind: typeSymbol.TypeKind,
                    Visibility: typeSymbol.DeclaredAccessibility);
            } catch (Exception e) {
                Logger.LogException(PIPELINE_NAME, generatorStepName, e);
                return null;
            }
        }
    }
}