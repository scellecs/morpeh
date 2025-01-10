namespace SourceGenerators.Generators.Systems {
    using System.Linq;
    using Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Utils.Caches;
    using Utils.NonSemantic;
    using Utils.Semantic;
    using Utils.Pools;

    [Generator]
    public class SystemSourceGenerator : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.SYSTEM_FULL_NAME,
                (s, _) => s is TypeDeclarationSyntax,
                (ctx, _) => (ctx.TargetNode as TypeDeclarationSyntax, ctx.TargetSymbol as INamedTypeSymbol, ctx.Attributes));
            
            context.RegisterSourceOutput(classes, static (spc, pair) => {
                var (typeDeclaration, typeSymbol, systemAttributes) = pair;
                if (typeDeclaration is null || typeSymbol is null) {
                    return;
                }
                
                if (!RunDiagnostics(spc, typeDeclaration)) {
                    return;
                }
                
                var attribute = systemAttributes.FirstOrDefault();
                if (attribute is null) {
                    return;
                }
                
                var args = attribute.ConstructorArguments;
                
                var skipCommit    = false;
                if (args.Length >= 1 && args[0].Value is bool skipCommitValue) {
                    skipCommit = skipCommitValue;
                }
                
                var alwaysEnabled = false;
                if (args.Length >= 2 && args[1].Value is bool alwaysEnabledValue) {
                    alwaysEnabled = alwaysEnabledValue;
                }

                var hasWorldProperty = false;
                
                var members = typeSymbol.GetMembers();
                for (int i = 0, length = members.Length; i < length; i++) {
                    if (members[i] is not IPropertySymbol propertySymbol) {
                        continue;
                    }

                    if (propertySymbol.Name != "World") {
                        continue;
                    }

                    hasWorldProperty = true;
                    break;
                }
                
                var typeName = typeDeclaration.Identifier.ToString();
                
                var stashes = ThreadStaticListCache<MorpehComponentHelpersSemantic.StashRequirement>.GetClear();
                MorpehComponentHelpersSemantic.FillStashRequirements(stashes, typeSymbol);
                
                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();

                sb.AppendMorpehDebugDefines();
                sb.AppendIndent(indent).AppendLine("using Scellecs.Morpeh;");
                sb.AppendBeginNamespace(typeDeclaration, indent).AppendLine();

                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent)
                    .AppendVisibility(typeDeclaration)
                    .Append(" partial ")
                    .AppendTypeDeclarationType(typeDeclaration)
                    .Append(' ')
                    .Append(typeName)
                    .AppendGenericParams(typeDeclaration)
                    .Append(" : Scellecs.Morpeh.ISystem ")
                    .AppendGenericConstraints(typeSymbol)
                    .AppendLine(" {");
                
                
                using (indent.Scope()) {
                    if (!hasWorldProperty) {
                        sb.AppendIndent(indent).AppendLine("public World World { get; }");
                    }
                    
                    sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                    sb.AppendIndent(indent).AppendLine("private bool _systemHasFailed;");
                    sb.AppendEndIfDefine();
                    
                    sb.AppendLine().AppendLine();
                    foreach (var stash in stashes) {
                        sb.AppendIndent(indent).Append("private readonly ").Append(stash.fieldTypeName).Append(' ').Append(stash.fieldName).AppendLine(";");
                    }
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).Append("public ").Append(typeName).AppendLine("(Scellecs.Morpeh.World world) {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, typeName, "Constructor", indent)) {
                            sb.AppendIndent(indent).AppendLine("World = world;");

                            foreach (var stash in stashes) {
                                sb.AppendIndent(indent).Append(stash.fieldName).Append(" = ").Append(stash.metadataClassName).AppendLine(".GetStash(world);");
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallAwake() {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, typeName, "Awake", indent)) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                            sb.AppendIndent(indent).AppendLine("try {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("OnAwake();");
                            }

                            sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(typeName).AppendLine(" system (OnAwake), the system will be disabled\");");
                                sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
                                sb.AppendIndent(indent).AppendLine("_systemHasFailed = true;");
                            }

                            sb.AppendIndent(indent).AppendLine("}");
                            sb.AppendElseDefine();
                            sb.AppendIndent(indent).AppendLine("OnAwake();");
                            sb.AppendEndIfDefine();

                            sb.AppendIndent(indent).AppendLine("World.Commit();");
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
                        
                        if (!alwaysEnabled) {
                            sb.AppendIndent(indent).AppendLine("if (!IsEnabled()) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("return;");
                            }
                            sb.AppendIndent(indent).AppendLine("}");
                        }

                        using (MorpehSyntax.ScopedProfile(sb, typeName, "OnUpdate", indent)) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                            sb.AppendIndent(indent).AppendLine("try {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("OnUpdate(deltaTime);");
                            }

                            sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(typeName).AppendLine(" system (OnUpdate), the system will be disabled\");");
                                sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
                                sb.AppendIndent(indent).AppendLine("_systemHasFailed = true;");
                            }

                            sb.AppendIndent(indent).AppendLine("}");
                            sb.AppendElseDefine();
                            sb.AppendIndent(indent).AppendLine("OnUpdate(deltaTime);");
                            sb.AppendEndIfDefine();

                            if (!skipCommit) {
                                sb.AppendIndent(indent).AppendLine("World.Commit();");
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");

                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallDispose() {");
                    using (indent.Scope()) {
                        using (MorpehSyntax.ScopedProfile(sb, typeName, "Dispose", indent)) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                            sb.AppendIndent(indent).AppendLine("try {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("Dispose();");
                            }

                            sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(typeName).AppendLine(" system (Dispose), the system will be disabled\");");
                                sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
                                sb.AppendIndent(indent).AppendLine("_systemHasFailed = true;");
                            }

                            sb.AppendIndent(indent).AppendLine("}");
                            sb.AppendElseDefine();
                            sb.AppendIndent(indent).AppendLine("Dispose();");
                            sb.AppendEndIfDefine();

                            sb.AppendIndent(indent).AppendLine("World.Commit();");
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                }
                
                sb.AppendIndent(indent).AppendLine("}");
                sb.AppendEndNamespace(typeDeclaration, indent);
                
                spc.AddSource($"{typeName}.system_{typeSymbol.GetFullyQualifiedNameHash()}.g.cs", sb.ToStringAndReturn());
                
                IndentSourcePool.Return(indent);
            });
        }
        
        private static bool RunDiagnostics(SourceProductionContext spc, TypeDeclarationSyntax typeDeclaration) {
            var success = true;
            
            if (typeDeclaration.IsDeclaredInsideAnotherType()) {
                Errors.ReportNestedDeclaration(spc, typeDeclaration);
                success = false;
            }
 
            return success;
        }
    }
}