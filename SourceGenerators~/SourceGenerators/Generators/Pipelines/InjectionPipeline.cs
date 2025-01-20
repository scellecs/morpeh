namespace SourceGenerators.Generators.Pipelines {
    using System;
    using System.Collections.Immutable;
    using System.Threading;
    using Injection;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using Utils.Caches;
    using Utils.Collections;
    using Utils.Logging;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    [Generator]
    public class InjectionPipeline : IIncrementalGenerator {
        private const string PIPELINE_NAME = nameof(InjectionPipeline);
        
        public void Initialize(IncrementalGeneratorInitializationContext context) {
#if MORPEH_SOURCEGEN_INJECTABLE_SCAN_SLOW
            var injections = context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (s, _) => s is ClassDeclarationSyntax,
                    transform: static (s, ct) => ExtractInjectionsToGenerate(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(PIPELINE_NAME, "injections_ExtractInjectionsToGenerate")
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS)
                .WithLogging(PIPELINE_NAME, "injections_RemoveNullPass");
#else
            var injections = context.SyntaxProvider.ForAttributeWithMetadataName(
                    MorpehAttributes.INJECTABLE_FULL_NAME,
                    predicate: static (s, _) => s is ClassDeclarationSyntax,
                    transform: static (s, ct) => ExtractInjectionsToGenerate(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(PIPELINE_NAME, "injections_ExtractInjectionsToGenerate")
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS)
                .WithLogging(PIPELINE_NAME, "injections_RemoveNullPass");
#endif

            var genericResolvers = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    MorpehAttributes.GENERIC_INJECTION_RESOLVER_ATTRIBUTE_FULL_NAME,
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, ct) => ExtractGenericResolver(ctx, ct))
                .Where(static resolver => resolver is not null)
                .Select(static (resolver, _) => resolver!.Value)
                .Collect()
                .Select(static (resolvers, _) => resolvers.ToImmutableDictionary(static resolver => resolver.BaseTypeName, static resolver => resolver.ResolverTypeName));
            
            context.RegisterSourceOutput(injections.Combine(genericResolvers), static (spc, pair) => InjectionSourceGenerator.Generate(spc, pair.Left, pair.Right));
        }
        
#if MORPEH_SOURCEGEN_INJECTABLE_SCAN_SLOW
        private static InjectionToGenerate? ExtractInjectionsToGenerate(GeneratorSyntaxContext ctx, CancellationToken ct) {
            const string generatorStepName = nameof(ExtractInjectionsToGenerate);
            
            ct.ThrowIfCancellationRequested();

            try {
                if (ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) is not INamedTypeSymbol typeSymbol) {
                    return null;
                }

                var fields = GetInjectionFields(typeSymbol);
                if (fields.Length == 0) {
                    return null;
                }
                
                Logger.Log(PIPELINE_NAME, generatorStepName, $"Transform: {typeSymbol.Name}");

                var (genericParams, genericConstraints) = GenericsSemantic.GetGenericParamsAndConstraints(typeSymbol);

                return new InjectionToGenerate(
                    Hierarchy: ParentType.FromTypeSymbol(typeSymbol), 
                    TypeName: typeSymbol.Name,
                    TypeNamespace: typeSymbol.GetNamespaceString(),
                    GenericParams: genericParams,
                    GenericConstraints: genericConstraints,
                    Fields: fields,
                    TypeKind: typeSymbol.TypeKind,
                    Visibility: typeSymbol.DeclaredAccessibility,
                    HasInjectionsInParents: HasInjectionsInParents(typeSymbol));
            } catch (Exception e) {
                Logger.LogException(PIPELINE_NAME, generatorStepName, e);
                return null;
            }
        }
#else
        private static InjectionToGenerate? ExtractInjectionsToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            const string generatorStepName = nameof(ExtractInjectionsToGenerate);
            
            ct.ThrowIfCancellationRequested();

            try {
                if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                    return null;
                }
                
                Logger.Log(PIPELINE_NAME, generatorStepName, $"Transform: {typeSymbol.Name}");

                var (genericParams, genericConstraints) = GenericsSemantic.GetGenericParamsAndConstraints(typeSymbol);

                return new InjectionToGenerate(
                    Hierarchy: ParentType.FromTypeSymbol(typeSymbol), 
                    TypeName: typeSymbol.Name,
                    TypeNamespace: typeSymbol.GetNamespaceString(),
                    GenericParams: genericParams,
                    GenericConstraints: genericConstraints,
                    Fields: GetInjectionFields(typeSymbol),
                    TypeKind: typeSymbol.TypeKind,
                    Visibility: typeSymbol.DeclaredAccessibility,
                    HasInjectionsInParents: HasInjectionsInParents(typeSymbol));
            } catch (Exception e) {
                Logger.LogException(PIPELINE_NAME, generatorStepName, e);
                return null;
            }
        }
#endif
        
        private static EquatableArray<InjectionField> GetInjectionFields(ITypeSymbol typeSymbol) {
            var fields = ThreadStaticListCache<InjectionField>.GetClear();
            
            var members = typeSymbol.GetMembers();

            for (int i = 0, length = members.Length; i < length; i++) {
                if (members[i] is not IFieldSymbol fieldSymbol || fieldSymbol.IsStatic) {
                    continue;
                }

                var attributes = fieldSymbol.GetAttributes();

                for (int j = 0, attributesLength = attributes.Length; j < attributesLength; j++) {
                    if (attributes[j].AttributeClass?.Name != MorpehAttributes.INJECTABLE_NAME) {
                        continue;
                    }

                    if (fieldSymbol.Type is not INamedTypeSymbol namedTypeSymbol) {
                        continue;
                    }

                    if (namedTypeSymbol.IsGenericType) {
                        fields.Add(new InjectionField(
                            Name: fieldSymbol.Name,
                            TypeName: namedTypeSymbol.ConstructUnboundGenericType().ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            GenericParams: StringBuilderPool.Get().AppendGenericParams(namedTypeSymbol).ToStringAndReturn()
                        ));
                    }
                    else {
                        fields.Add(new InjectionField(
                            Name: fieldSymbol.Name,
                            TypeName: fieldSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            GenericParams: null
                        ));
                    }
                }
            }
            
            return new EquatableArray<InjectionField>(fields);
        }
        
        private static bool HasInjectionsInParents(INamedTypeSymbol typeSymbol) {
            var currentSymbol = typeSymbol.BaseType;

            while (currentSymbol is { TypeKind : TypeKind.Class }) {
#if MORPEH_SOURCEGEN_INJECTABLE_SCAN_SLOW
                var members = currentSymbol.GetMembers();

                for (int i = 0, length = members.Length; i < length; i++) {
                    var member = members[i];
                    if (member is not IFieldSymbol fieldSymbol || fieldSymbol.IsStatic) {
                        continue;
                    }

                    var attributes = fieldSymbol.GetAttributes();
                    for (int j = 0, jlength = attributes.Length; j < jlength; j++) {
                        if (attributes[j].AttributeClass?.Name == MorpehAttributes.INJECTABLE_NAME) {
                            return true;
                        }
                    }
                }
#else
                var attributes = currentSymbol.GetAttributes();
                for (int i = 0, length = attributes.Length; i < length; i++) {
                    if (attributes[i].AttributeClass?.Name == MorpehAttributes.INJECTABLE_NAME) {
                        return true;
                    }
                }
#endif

                currentSymbol = currentSymbol.BaseType;
            }


            return false;
        }
        
        private static GenericResolver? ExtractGenericResolver(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();

            try {
                if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                    return null;
                }
                
                Logger.Log(PIPELINE_NAME, nameof(ExtractGenericResolver), $"Transform: {typeSymbol.Name}");

                var args = ctx.Attributes[0].ConstructorArguments;
                if (args.Length != 1 || args[0].Value is not INamedTypeSymbol baseType) {
                    return null;
                }

                if (!baseType.IsUnboundGenericType) {
                    return null;
                }
                
                return new GenericResolver(baseType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            } catch (Exception e) {
                Logger.LogException(PIPELINE_NAME, nameof(ExtractGenericResolver), e);
                return null;
            }
        }
    }
}