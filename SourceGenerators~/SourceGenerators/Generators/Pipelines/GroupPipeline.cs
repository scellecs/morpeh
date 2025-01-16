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

    [Generator]
    public class GroupPipeline : IIncrementalGenerator {
        private const string PIPELINE_NAME = nameof(GroupPipeline);
        
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var groups = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.SYSTEMS_GROUP_FULL_NAME,
                predicate: static (s, _) => s is TypeDeclarationSyntax syntaxNode && syntaxNode.Parent is not TypeDeclarationSyntax,
                transform: static (ctx, ct) => ExtractSystemsGroupsToGenerate(ctx, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(PIPELINE_NAME, "systemsgroup_ExtractSystemsGroupsToGenerate")
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS)
                .WithLogging(PIPELINE_NAME, "systemsgroup_RemoveNullPass");
        
            // TODO: DTOs

            var runners = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.SYSTEMS_GROUP_RUNNER_FULL_NAME,
                predicate: (s, _) => s is ClassDeclarationSyntax syntaxNode && syntaxNode.Parent is not TypeDeclarationSyntax,
                transform: (ctx, _) => (ctx.TargetNode as ClassDeclarationSyntax, ctx.TargetSymbol as INamedTypeSymbol))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .WithLogging(nameof(GroupPipeline), "runners_ExtractRunnersToGenerate");

            context.RegisterSourceOutput(groups, static (spc, pair) => SystemsGroupSourceGenerator.Generate(spc, pair));
            context.RegisterSourceOutput(runners, static (spc, pair) => SystemsGroupRunnerSourceGenerator.Generate(spc, pair));
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

                var members = typeSymbol.GetMembers();
                for (int i = 0, length = members.Length; i < length; i++) {
                    if (members[i] is not IFieldSymbol fieldSymbol) {
                        continue;
                    }
                    
                    var typeAttributes  = fieldSymbol.Type.GetAttributes();

                    var fieldKind = SystemsGroupFieldKind.Constructible;
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
                                } else {
                                    registerSymbol = fieldSymbol.Type as INamedTypeSymbol;
                                }
                            
                                registerAs = registerSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
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
                
                var (genericParams, genericConstraints) = GenericsSemantic.GetGenericParamsAndConstraints(typeSymbol);

                return new SystemsGroupToGenerate(
                    TypeName: typeSymbol.Name,
                    TypeNamespace: typeSymbol.GetNamespaceString(),
                    GenericParams: genericParams,
                    GenericConstraints: genericConstraints,
                    Fields: new EquatableArray<SystemsGroupField>(systemsGroupFields),
                    TypeKind: typeSymbol.TypeKind,
                    Visibility: typeSymbol.DeclaredAccessibility,
                    InlineUpdateMethods: inlineUpdateMethods);
            } catch (Exception e) {
                Logger.LogException(PIPELINE_NAME, generatorStepName, e);
                return null;
            }
        }
        
#if MORPEH_SOURCEGEN_INJECTABLE_SCAN_SLOW
        private static bool IsInjectable(ITypeSymbol typeSymbol) {
            var currentSymbol  = typeSymbol;

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

                currentSymbol  = currentSymbol.BaseType;
            }

            return false;
        }
#endif
    }
}