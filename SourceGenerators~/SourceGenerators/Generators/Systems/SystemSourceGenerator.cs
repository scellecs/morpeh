namespace SourceGenerators.Generators.Systems {
    using System;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Utils.Caches;
    using Utils.Collections;
    using Utils.NonSemantic;
    using Utils.Semantic;
    using Utils.Pools;

    [Generator]
    public class SystemSourceGenerator : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.SYSTEM_FULL_NAME,
                predicate: static (s, _) => s is TypeDeclarationSyntax syntaxNode && syntaxNode.Parent is not TypeDeclarationSyntax,
                transform: static (s, ct) => ExtractTypesToGenerate(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS);
            
            context.RegisterSourceOutput(classes, static (spc, system) => {
                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();

                sb.AppendMorpehDebugDefines();
                
                if (system.typeNamespace != null) {
                    sb.AppendIndent(indent).Append("namespace ").Append(system.typeNamespace).AppendLine(" {");
                    indent.Right();
                }

                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent)
                    .Append(Types.GetVisibilityModifierString(system.visibility))
                    .Append(" partial ")
                    .Append(Types.AsString(system.typeDeclType))
                    .Append(' ')
                    .Append(system.typeName)
                    .Append(system.genericParams)
                    .Append(" : Scellecs.Morpeh.IInitializer ")
                    .Append(system.genericConstraints)
                    .AppendLine(" {");
                
                
                using (indent.Scope()) {
                    sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                    sb.AppendIndent(indent).AppendLine("private bool _systemHasFailed;");
                    sb.AppendEndIfDefine();
                    
                    sb.AppendLine().AppendLine();
                    foreach (var stash in system.stashRequirements) {
                        sb.AppendIndent(indent).Append("private readonly ").Append(stash.fieldTypeName).Append(' ').Append(stash.fieldName).AppendLine(";");
                    }
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).Append("public ").Append(system.typeName).AppendLine("(Scellecs.Morpeh.World world) {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, system.typeName, "Constructor", indent)) {
                            sb.AppendIndent(indent).AppendLine("World = world;");

                            foreach (var stash in system.stashRequirements) {
                                sb.AppendIndent(indent).Append(stash.fieldName).Append(" = ").Append(stash.metadataClassName).AppendLine(".GetStash(world);");
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallAwake() {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, system.typeName, "Awake", indent)) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                            sb.AppendIndent(indent).AppendLine("try {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("OnAwake();");
                            }

                            sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(system.typeName).AppendLine(" system (OnAwake), the system will be disabled\");");
                                sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
                                sb.AppendIndent(indent).AppendLine("_systemHasFailed = true;");
                            }

                            sb.AppendIndent(indent).AppendLine("}");
                            sb.AppendElseDefine();
                            sb.AppendIndent(indent).AppendLine("OnAwake();");
                            sb.AppendEndIfDefine();

                            sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.WorldExtensions.Commit(World);");
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallUpdate(float deltaTime) {");
                    using (indent.Scope()) {
                        sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                        sb.AppendIndent(indent).AppendLine("if (_systemHasFailed) {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("return;");
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                        sb.AppendEndIfDefine();
                        
                        if (!system.alwaysEnabled) {
                            sb.AppendIndent(indent).AppendLine("if (!IsEnabled()) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("return;");
                            }
                            sb.AppendIndent(indent).AppendLine("}");
                        }

                        using (MorpehSyntax.ScopedProfile(sb, system.typeName, "OnUpdate", indent)) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                            sb.AppendIndent(indent).AppendLine("try {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("OnUpdate(deltaTime);");
                            }

                            sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(system.typeName).AppendLine(" system (OnUpdate), the system will be disabled\");");
                                sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
                                sb.AppendIndent(indent).AppendLine("_systemHasFailed = true;");
                            }

                            sb.AppendIndent(indent).AppendLine("}");
                            sb.AppendElseDefine();
                            sb.AppendIndent(indent).AppendLine("OnUpdate(deltaTime);");
                            sb.AppendEndIfDefine();

                            if (!system.skipCommit) {
                                sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.WorldExtensions.Commit(World);");
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");

                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallDispose() {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, system.typeName, "Dispose", indent)) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                            sb.AppendIndent(indent).AppendLine("try {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("Dispose();");
                            }

                            sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(system.typeName).AppendLine(" system (Dispose), the system will be disabled\");");
                                sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
                                sb.AppendIndent(indent).AppendLine("_systemHasFailed = true;");
                            }

                            sb.AppendIndent(indent).AppendLine("}");
                            sb.AppendElseDefine();
                            sb.AppendIndent(indent).AppendLine("Dispose();");
                            sb.AppendEndIfDefine();

                            sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.WorldExtensions.Commit(World);");
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                }
                
                sb.AppendIndent(indent).AppendLine("}");
                
                if (system.typeNamespace != null) {
                    indent.Left();
                    sb.AppendIndent(indent).AppendLine("}");
                }
                
                spc.AddSource($"{system.typeName}.system_{Guid.NewGuid():N}.g.cs", sb.ToStringAndReturn());
                
                IndentSourcePool.Return(indent);
            });
        }
        
        private static SystemToGenerate? ExtractTypesToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            
            if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                return null;
            }
            
            var syntaxNode = (TypeDeclarationSyntax)ctx.TargetNode;
            
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
            
            var args = ctx.Attributes.First().ConstructorArguments;
                
            var skipCommit    = false;
            if (args.Length >= 1 && args[0].Value is bool skipCommitValue) {
                skipCommit = skipCommitValue;
            }
                
            var alwaysEnabled = false;
            if (args.Length >= 2 && args[1].Value is bool alwaysEnabledValue) {
                alwaysEnabled = alwaysEnabledValue;
            }

            var stashRequirements = ThreadStaticListCache<StashRequirement>.GetClear();
            MorpehComponentHelpersSemantic.FillStashRequirements(stashRequirements, typeSymbol);

            return new SystemToGenerate(
                typeName: syntaxNode.Identifier.ToString(),
                typeNamespace: typeNamespace,
                genericParams: genericParams,
                genericConstraints: genericConstraints,
                stashRequirements: new EquatableArray<StashRequirement>(stashRequirements),
                typeDeclType: Types.TypeDeclTypeFromSyntaxNode(syntaxNode),
                visibility: Types.GetVisibilityModifier(syntaxNode),
                skipCommit: skipCommit,
                alwaysEnabled: alwaysEnabled);
        }
    }
}