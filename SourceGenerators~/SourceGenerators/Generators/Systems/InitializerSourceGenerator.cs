namespace SourceGenerators.Generators.Systems {
    using System;
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

                if (initializer.typeNamespace != null) {
                    sb.AppendIndent(indent).Append("namespace ").Append(initializer.typeNamespace).AppendLine(" {");
                    indent.Right();
                }

                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent)
                    .Append(Types.GetVisibilityModifierString(initializer.visibility))
                    .Append(" partial ")
                    .Append(Types.AsString(initializer.typeDeclType))
                    .Append(' ')
                    .Append(initializer.typeName)
                    .Append(initializer.genericParams)
                    .Append(" : Scellecs.Morpeh.IInitializer ")
                    .Append(initializer.genericConstraints)
                    .AppendLine(" {");
                
                
                using (indent.Scope()) {
                    sb.AppendLine().AppendLine();
                    foreach (var stash in initializer.stashRequirements) {
                        sb.AppendIndent(indent).Append("private readonly ").Append(stash.fieldTypeName).Append(' ').Append(stash.fieldName).AppendLine(";");
                    }
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).Append("public ").Append(initializer.typeName).AppendLine("(Scellecs.Morpeh.World world) {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, initializer.typeName, "Constructor", indent)) {
                            sb.AppendIndent(indent).AppendLine("World = world;");
                        
                            foreach (var stash in initializer.stashRequirements) {
                                sb.AppendIndent(indent).Append(stash.fieldName).Append(" = ").Append(stash.metadataClassName).AppendLine(".GetStash(world);");
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallAwake() {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, initializer.typeName, "Awake", indent)) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                            sb.AppendIndent(indent).AppendLine("try {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("OnAwake();");
                            }

                            sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(initializer.typeName).AppendLine(" initializer (OnAwake)\");");
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
                        using (MorpehSyntax.ScopedProfile(sb, initializer.typeName, "Dispose", indent)) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                            sb.AppendIndent(indent).AppendLine("try {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("Dispose();");
                            }

                            sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(initializer.typeName).AppendLine(" initializer (Dispose)\");");
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
                if (initializer.typeNamespace != null) {
                    indent.Left();
                    sb.AppendIndent(indent).AppendLine("}");
                }
                
                // TODO: Think of a better way to handle collisions between names.
                spc.AddSource($"{initializer.typeName}.initializer_{Guid.NewGuid():N}.g.cs", sb.ToStringAndReturn());
                
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

            var stashRequirements = ThreadStaticListCache<StashRequirement>.GetClear();
            MorpehComponentHelpersSemantic.FillStashRequirements(stashRequirements, typeSymbol);

            return new InitializerToGenerate(
                typeName: syntaxNode.Identifier.ToString(),
                typeNamespace: typeNamespace,
                genericParams: genericParams,
                genericConstraints: genericConstraints,
                stashRequirements: new EquatableArray<StashRequirement>(stashRequirements),
                typeDeclType: Types.TypeDeclTypeFromSyntaxNode(syntaxNode),
                visibility: Types.GetVisibilityModifier(syntaxNode));
        }
    }
}