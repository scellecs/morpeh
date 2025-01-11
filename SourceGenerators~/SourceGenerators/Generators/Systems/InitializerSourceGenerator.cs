namespace SourceGenerators.Generators.Systems {
    using System;
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
    public class InitializerSourceGenerator : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.INITIALIZER_FULL_NAME,
                predicate: static (s, _) => s is TypeDeclarationSyntax syntaxNode && syntaxNode.Parent is not TypeDeclarationSyntax,
                transform: static (s, ct) => ExtractTypesToGenerate(s, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .Where(static candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS);
            
            context.RegisterSourceOutput(classes, static (spc, initializer) => {
                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                sb.AppendMorpehDebugDefines();

                if (initializer.TypeNamespace != null) {
                    sb.AppendIndent(indent).Append("namespace ").Append(initializer.TypeNamespace).AppendLine(" {");
                    indent.Right();
                }

                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent)
                    .Append(Types.GetVisibilityModifierString(initializer.Visibility))
                    .Append(" partial ")
                    .Append(Types.AsString(initializer.TypeDeclType))
                    .Append(' ')
                    .Append(initializer.TypeName)
                    .Append(initializer.GenericParams)
                    .Append(" : Scellecs.Morpeh.IInitializer ")
                    .Append(initializer.GenericConstraints)
                    .AppendLine(" {");
                
                
                using (indent.Scope()) {
                    if (initializer.StashRequirements.Length > 0) {
                        sb.AppendLine().AppendLine();
                        foreach (var stash in initializer.StashRequirements) {
                            sb.AppendIndent(indent).Append("private readonly ").Append(stash.FieldTypeName).Append(' ').Append(stash.FieldName).AppendLine(";");
                        }
                    }
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).Append("public ").Append(initializer.TypeName).AppendLine("(Scellecs.Morpeh.World world) {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, initializer.TypeName, "Constructor", indent)) {
                            sb.AppendIndent(indent).AppendLine("World = world;");
                        
                            foreach (var stash in initializer.StashRequirements) {
                                sb.AppendIndent(indent).Append(stash.FieldName).Append(" = ").Append(stash.MetadataClassName).AppendLine(".GetStash(world);");
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallAwake() {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, initializer.TypeName, "Awake", indent)) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                            sb.AppendIndent(indent).AppendLine("try {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("OnAwake();");
                            }

                            sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(initializer.TypeName).AppendLine(" initializer (OnAwake)\");");
                                sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
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
                    sb.AppendIndent(indent).AppendLine("public void CallDispose() {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, initializer.TypeName, "Dispose", indent)) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                            sb.AppendIndent(indent).AppendLine("try {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("Dispose();");
                            }

                            sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(initializer.TypeName).AppendLine(" initializer (Dispose)\");");
                                sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
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
                if (initializer.TypeNamespace != null) {
                    indent.Left();
                    sb.AppendIndent(indent).AppendLine("}");
                }
                
                // TODO: Think of a better way to handle collisions between names.
                spc.AddSource($"{initializer.TypeName}.initializer_{Guid.NewGuid():N}.g.cs", sb.ToStringAndReturn());
                
                IndentSourcePool.Return(indent);
            });
        }

        private static InitializerToGenerate? ExtractTypesToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
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
            
            return new InitializerToGenerate(
                TypeName: syntaxNode.Identifier.ToString(),
                TypeNamespace: typeNamespace,
                GenericParams: genericParams,
                GenericConstraints: genericConstraints,
                StashRequirements: MorpehComponentHelpersSemantic.GetStashRequirements(typeSymbol),
                TypeDeclType: Types.TypeDeclTypeFromSyntaxNode(syntaxNode),
                Visibility: Types.GetVisibilityModifier(syntaxNode));
        }

        private record struct InitializerToGenerate(
            string TypeName,
            string? TypeNamespace,
            string GenericParams,
            string GenericConstraints,
            EquatableArray<StashRequirement> StashRequirements,
            TypeDeclType TypeDeclType,
            SyntaxKind Visibility);
    }
}