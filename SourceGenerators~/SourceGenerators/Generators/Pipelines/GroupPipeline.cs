namespace SourceGenerators.Generators.Pipelines {
    using System;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using SystemsGroup;
    using SystemsGroupRunner;
    using Utils.Caches;
    using Utils.Collections;
    using Utils.Logging;
    using Utils.NonSemantic;
    using Utils.Semantic;
    using System.Linq;
    using Options;

    [Generator]
    public class GroupPipeline : IIncrementalGenerator {
        private const string PIPELINE_NAME = nameof(GroupPipeline);

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var options = context.ParseOptionsProvider
                .Select(static (parseOptions, _) => PreprocessorOptionsData.FromParseOptions(parseOptions));
            
            // TODO: Support groups & runners without update methods (attribute argument)
            
            var groups = context.SyntaxProvider.ForAttributeWithMetadataName(
                    MorpehAttributes.SYSTEMS_GROUP_FULL_NAME,
                    predicate: static (s, _) => s is TypeDeclarationSyntax,
                    transform: static (ctx, ct) => ExtractSystemsGroupsToGenerate(ctx, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(PIPELINE_NAME, "systemsgroup_ExtractSystemsGroupsToGenerate")
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS)
                .WithLogging(PIPELINE_NAME, "systemsgroup_RemoveNullPass");
            
            var runners = context.SyntaxProvider.ForAttributeWithMetadataName(
                    MorpehAttributes.SYSTEMS_GROUP_RUNNER_FULL_NAME,
                    predicate: static (s, _) => s is ClassDeclarationSyntax,
                    transform: static (ctx, ct) => ExtractRunnersToGenerate(ctx, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(PIPELINE_NAME, "runner_ExtractSystemsGroupsToGenerate")
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS)
                .WithLogging(PIPELINE_NAME, "runner_RemoveNullPass");

            context.RegisterSourceOutput(groups.Combine(options), static (spc, pair) => SystemsGroupSourceGenerator.Generate(spc, pair.Left, pair.Right));
            context.RegisterSourceOutput(runners.Combine(options), static (spc, pair) => SystemsGroupRunnerSourceGenerator.Generate(spc, pair.Left, pair.Right));
        }

        private static SystemsGroupToGenerate? ExtractSystemsGroupsToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            const string generatorStepName = nameof(ExtractSystemsGroupsToGenerate);

            ct.ThrowIfCancellationRequested();

            try {
                if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                    return null;
                }

                Logger.Log(PIPELINE_NAME, generatorStepName, $"Transform: {typeSymbol.Name}");
                
                var systemsGroupFields = ThreadStaticListCache<SystemsGroupField>.GetClear();
                var hasRegistrations   = false;

                var members = typeSymbol.GetMembers();
                for (int i = 0, length = members.Length; i < length; i++) {
                    if (members[i] is not IFieldSymbol fieldSymbol) {
                        continue;
                    }

                    var typeAttributes = fieldSymbol.Type.GetAttributes();

                    var     fieldKind  = SystemsGroupFieldKind.Constructible;
                    string? registerAs = null;

                    var isInjectable = false;
#if MORPEH_SOURCEGEN_INJECTABLE_SCAN_SLOW
                    isInjectable = IsInjectable(fieldSymbol.Type);
#endif

                    for (int j = 0, jlength = typeAttributes.Length; j < jlength; j++) {
                        var attribute     = typeAttributes[j];
                        var attributeName = attribute.AttributeClass?.Name;

                        switch (attributeName) {
                            case MorpehAttributes.SYSTEM_NAME: {
                                fieldKind = SystemsGroupFieldKind.System;
                                break;
                            }
                            case MorpehAttributes.INITIALIZER_NAME: {
                                fieldKind = SystemsGroupFieldKind.Initializer;
                                break;
                            }
                            case MorpehAttributes.REGISTER_NAME: {
                                INamedTypeSymbol? registerSymbol;

                                var attributeArgs = attribute.ConstructorArguments;
                                if (attributeArgs.Length > 0 && attributeArgs[0].Value is INamedTypeSymbol registerSymbolArg) {
                                    registerSymbol = registerSymbolArg;
                                }
                                else {
                                    registerSymbol = fieldSymbol.Type as INamedTypeSymbol;
                                }

                                registerAs = registerSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                                hasRegistrations = true;
                                
                                break;
                            }
#if !MORPEH_SOURCEGEN_INJECTABLE_SCAN_SLOW
                            case MorpehAttributes.INJECTABLE_NAME: {
                                isInjectable = true;
                                break;
                            }
#endif
                        }
                    }

                    if (fieldKind == SystemsGroupFieldKind.Constructible) {
                        if (fieldSymbol.Type.AllInterfaces.Any(x => x.Name == KnownTypes.DISPOSABLE_NAME && x.ToDisplayString() == KnownTypes.DISPOSABLE_FULL_NAME)) {
                            fieldKind = SystemsGroupFieldKind.Disposable;
                        }
                    }

                    systemsGroupFields.Add(new SystemsGroupField(
                        Name: fieldSymbol.Name,
                        TypeName: fieldSymbol.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        RegisterAs: registerAs,
                        FieldKind: fieldKind,
                        IsInjectable: isInjectable));
                }

                var inlineUpdateMethods = false;

                var args = ctx.Attributes[0].ConstructorArguments;
                if (args.Length >= 1 && args[0].Value is bool inlineUpdateMethodsValue) {
                    inlineUpdateMethods = inlineUpdateMethodsValue;
                }
                
                return new SystemsGroupToGenerate(
                    Hierarchy: ParentType.FromTypeSymbol(typeSymbol), 
                    TypeName: typeSymbol.Name,
                    TypeNamespace: typeSymbol.GetNamespaceString(),
                    Fields: new EquatableArray<SystemsGroupField>(systemsGroupFields),
                    TypeKind: typeSymbol.TypeKind,
                    Visibility: typeSymbol.DeclaredAccessibility,
                    HasRegistrations: hasRegistrations,
                    InlineUpdateMethods: inlineUpdateMethods);
            }
            catch (Exception e) {
                Logger.LogException(PIPELINE_NAME, generatorStepName, e);
                return null;
            }
        }

#if MORPEH_SOURCEGEN_INJECTABLE_SCAN_SLOW
        private static bool IsInjectable(ITypeSymbol typeSymbol) {
            var currentSymbol = typeSymbol;

            while (currentSymbol is { TypeKind: TypeKind.Class }) {
                var members = currentSymbol.GetMembers();
                
                for (int i = 0, length = members.Length; i < length; i++) {
                    if (members[i] is not IFieldSymbol fieldSymbol || fieldSymbol.IsStatic) {
                        continue;
                    }

                    var attributes = fieldSymbol.GetAttributes();
                    
                    for (int j = 0, attributesLength = attributes.Length; j < attributesLength; j++) {
                        if (attributes[j].AttributeClass?.Name == MorpehAttributes.INJECTABLE_NAME) {
                            return true;
                        }
                    }
                }

                currentSymbol = currentSymbol.BaseType;
            }

            return false;
        }
#endif

        private static RunnerToGenerate? ExtractRunnersToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            const string generatorStepName = nameof(ExtractRunnersToGenerate);

            ct.ThrowIfCancellationRequested();

            try {
                if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                    return null;
                }

                Logger.Log(PIPELINE_NAME, generatorStepName, $"Transform: {typeSymbol.Name}");
                
                var fields = ThreadStaticListCache<RunnerField>.GetClear();
                
                var typeMembers = typeSymbol.GetMembers();
                for (int i = 0, length = typeMembers.Length; i < length; i++) {
                    if (typeMembers[i] is not IFieldSymbol fieldSymbol) {
                        continue;
                    }

                    if (fieldSymbol.Type is not INamedTypeSymbol fieldTypeSymbol) {
                        continue;
                    }

                    fields.Add(new RunnerField(
                        Name: fieldSymbol.Name,
                        TypeName: fieldTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
                }
                
                return new RunnerToGenerate(
                    Hierarchy: ParentType.FromTypeSymbol(typeSymbol),
                    TypeName: typeSymbol.Name,
                    TypeNamespace: typeSymbol.GetNamespaceString(),
                    Fields: new EquatableArray<RunnerField>(fields),
                    TypeKind: typeSymbol.TypeKind,
                    Visibility: typeSymbol.DeclaredAccessibility);
            } catch (Exception e) {
                Logger.LogException(PIPELINE_NAME, generatorStepName, e);
                return null;
            }
        }
    }
}