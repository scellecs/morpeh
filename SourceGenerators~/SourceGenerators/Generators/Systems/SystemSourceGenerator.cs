namespace SourceGenerators.Generators.Systems {
    using System;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Utils.Collections;
    using Utils.NonSemantic;
    using Utils.Semantic;
    using Utils.Pools;

    // TODO: Move to a separate pipeline
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
                
                if (system.TypeNamespace != null) {
                    sb.AppendIndent(indent).Append("namespace ").Append(system.TypeNamespace).AppendLine(" {");
                    indent.Right();
                }

                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent)
                    .Append(Types.GetVisibilityModifierString(system.Visibility))
                    .Append(" partial ")
                    .Append(Types.AsString(system.TypeDeclType))
                    .Append(' ')
                    .Append(system.TypeName)
                    .Append(system.GenericParams)
                    .Append(" : Scellecs.Morpeh.IInitializer ")
                    .Append(system.GenericConstraints)
                    .AppendLine(" {");
                
                
                using (indent.Scope()) {
                    sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                    sb.AppendIndent(indent).AppendLine("private bool _systemHasFailed;");
                    sb.AppendEndIfDefine();

                    if (system.StashRequirements.Length > 0) {
                        sb.AppendLine().AppendLine();
                        foreach (var stash in system.StashRequirements) {
                            sb.AppendIndent(indent).Append("private readonly ").Append(stash.FieldTypeName).Append(' ').Append(stash.FieldName).AppendLine(";");
                        }
                    }

                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).Append("public ").Append(system.TypeName).AppendLine("(Scellecs.Morpeh.World world) {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, system.TypeName, "Constructor", indent)) {
                            sb.AppendIndent(indent).AppendLine("World = world;");

                            foreach (var stash in system.StashRequirements) {
                                sb.AppendIndent(indent).Append(stash.FieldName).Append(" = ").Append(stash.MetadataClassName).AppendLine(".GetStash(world);");
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallAwake() {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, system.TypeName, "Awake", indent)) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                            sb.AppendIndent(indent).AppendLine("try {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("OnAwake();");
                            }

                            sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(system.TypeName).AppendLine(" system (OnAwake), the system will be disabled\");");
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
                        
                        if (!system.AlwaysEnabled) {
                            sb.AppendIndent(indent).AppendLine("if (!IsEnabled()) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("return;");
                            }
                            sb.AppendIndent(indent).AppendLine("}");
                        }

                        using (MorpehSyntax.ScopedProfile(sb, system.TypeName, "OnUpdate", indent)) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                            sb.AppendIndent(indent).AppendLine("try {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("OnUpdate(deltaTime);");
                            }

                            sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(system.TypeName).AppendLine(" system (OnUpdate), the system will be disabled\");");
                                sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
                                sb.AppendIndent(indent).AppendLine("_systemHasFailed = true;");
                            }

                            sb.AppendIndent(indent).AppendLine("}");
                            sb.AppendElseDefine();
                            sb.AppendIndent(indent).AppendLine("OnUpdate(deltaTime);");
                            sb.AppendEndIfDefine();

                            if (!system.SkipCommit) {
                                sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.WorldExtensions.Commit(World);");
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");

                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallDispose() {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, system.TypeName, "Dispose", indent)) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                            sb.AppendIndent(indent).AppendLine("try {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("Dispose();");
                            }

                            sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(system.TypeName).AppendLine(" system (Dispose), the system will be disabled\");");
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
                
                if (system.TypeNamespace != null) {
                    indent.Left();
                    sb.AppendIndent(indent).AppendLine("}");
                }
                
                spc.AddSource($"{system.TypeName}.system_{Guid.NewGuid():N}.g.cs", sb.ToStringAndReturn());
                
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
            
            var args = ctx.Attributes[0].ConstructorArguments;
                
            var skipCommit    = false;
            if (args.Length >= 1 && args[0].Value is bool skipCommitValue) {
                skipCommit = skipCommitValue;
            }
                
            var alwaysEnabled = false;
            if (args.Length >= 2 && args[1].Value is bool alwaysEnabledValue) {
                alwaysEnabled = alwaysEnabledValue;
            }

            return new SystemToGenerate(
                TypeName: syntaxNode.Identifier.ToString(),
                TypeNamespace: typeNamespace,
                GenericParams: genericParams,
                GenericConstraints: genericConstraints,
                StashRequirements: MorpehComponentHelpersSemantic.GetStashRequirements(typeSymbol),
                TypeDeclType: Types.TypeDeclTypeFromSyntaxNode(syntaxNode),
                Visibility: Types.GetVisibilityModifier(syntaxNode),
                SkipCommit: skipCommit,
                AlwaysEnabled: alwaysEnabled);
        }
        
        private record struct SystemToGenerate(
            string TypeName,
            string? TypeNamespace,
            string GenericParams,
            string GenericConstraints,
            EquatableArray<StashRequirement> StashRequirements,
            TypeDeclType TypeDeclType,
            SyntaxKind Visibility,
            bool SkipCommit,
            bool AlwaysEnabled);
    }
}