namespace SourceGenerators {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Utils;

    // TODO: Systems disable mechanism for exceptions.
    // TODO: IsEnabled() method to check system enter condition.
    [Generator]
    public class SystemSourceGenerator : IIncrementalGenerator {
        private const string ATTRIBUTE_NAME = "System";
        private const string ALWAYS_ENABLED_ATTRIBUTE_NAME = "AlwaysEnabled";
        private const string SKIP_COMMIT_ATTRIBUTE_NAME = "SkipCommit";
        
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var classes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is TypeDeclarationSyntax typeDeclaration &&
                                                typeDeclaration.AttributeLists.Any(x => x.Attributes.Any(y => y?.Name.ToString() == ATTRIBUTE_NAME)),
                    transform: static (ctx, _) => (TypeDeclarationSyntax)ctx.Node)
                .Where(static typeDeclaration => typeDeclaration is not null);
            
            context.RegisterSourceOutput(classes, static (spc, typeDeclaration) =>
            {
                var typeName = typeDeclaration.Identifier.ToString();
                var alwaysEnabled = typeDeclaration.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString() == ALWAYS_ENABLED_ATTRIBUTE_NAME);
                var skipCommit = typeDeclaration.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString() == SKIP_COMMIT_ATTRIBUTE_NAME);

                var stashes = ComponentHelpers.GetStashRequirements(typeDeclaration);
                
                var sb     = StringBuilderPool.Get();
                var indent = IndentSource.GetThreadSingleton();

                sb.AppendMorpehDebugDefines();
                sb.AppendUsings(typeDeclaration, indent).AppendLine();
                sb.AppendBeginNamespace(typeDeclaration, indent).AppendLine();

                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent)
                    .AppendVisibility(typeDeclaration)
                    .Append(" partial ")
                    .AppendTypeDeclarationType(typeDeclaration)
                    .Append(' ')
                    .Append(typeName)
                    .AppendGenericParams(typeDeclaration)
                    .Append(" : ISystem ")
                    .AppendGenericConstraints(typeDeclaration)
                    .AppendLine(" {");
                
                
                using (indent.Scope()) {
                    sb.AppendIndent(indent).AppendLine("public World World { get; set; }");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void SetupRequirements() {");
                    using (indent.Scope()) {
                        sb.AppendIfDefine(Defines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).Append("MLogger.BeginSample(\"").Append(typeName).AppendLine("_SetupRequirements\");");
                        sb.AppendEndIfDefine();
                        
                        foreach (var stash in stashes) {
                            sb.AppendIndent(indent).Append(stash.fieldName).Append(" = ").Append(stash.metadataClass).AppendLine(".GetStash(World);");
                        }
                        
                        sb.AppendIfDefine(Defines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).AppendLine("MLogger.EndSample();");
                        sb.AppendEndIfDefine();
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallAwake() {");
                    using (indent.Scope()) {
                        sb.AppendIfDefine(Defines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).Append("MLogger.BeginSample(\"").Append(typeName).AppendLine("_Awake\");");
                        sb.AppendEndIfDefine();
                        
                        sb.AppendIfDefine(Defines.MORPEH_DEBUG);
                        sb.AppendIndent(indent).AppendLine("try {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("OnAwake();");
                        }
                        sb.AppendIndent(indent).AppendLine("} catch (Exception exception) {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).Append("MLogger.LogError(\"Exception in ").Append(typeName).AppendLine(" system (OnAwake), the system will be disabled\");");
                            sb.AppendIndent(indent).AppendLine("MLogger.LogException(exception);");
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                        sb.AppendElseDefine();
                        sb.AppendIndent(indent).AppendLine("OnAwake();");
                        sb.AppendEndIfDefine();
                        
                        sb.AppendIndent(indent).AppendLine("World.Commit();");
                        
                        sb.AppendIfDefine(Defines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).AppendLine("MLogger.EndSample();");
                        sb.AppendEndIfDefine();
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallUpdate(float deltaTime) {");
                    using (indent.Scope()) {
                        if (!alwaysEnabled) {
                            sb.AppendIndent(indent).AppendLine("if (!IsEnabled()) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("return;");
                            }
                            sb.AppendIndent(indent).AppendLine("}");
                        }
                        
                        sb.AppendIfDefine(Defines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).Append("MLogger.BeginSample(\"").Append(typeName).AppendLine("_OnUpdate\");");
                        sb.AppendEndIfDefine();
                        
                        sb.AppendIfDefine(Defines.MORPEH_DEBUG);
                        sb.AppendIndent(indent).AppendLine("try {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("OnUpdate(float deltaTime);");
                        }
                        sb.AppendIndent(indent).AppendLine("} catch (Exception exception) {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).Append("MLogger.LogError(\"Exception in ").Append(typeName).AppendLine(" system (OnUpdate), the system will be disabled\");");
                            sb.AppendIndent(indent).AppendLine("MLogger.LogException(exception);");
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                        sb.AppendElseDefine();
                        sb.AppendIndent(indent).AppendLine("OnUpdate(deltaTime);");
                        sb.AppendEndIfDefine();

                        if (!skipCommit) {
                            sb.AppendIndent(indent).AppendLine("World.Commit();");
                        }
                        
                        sb.AppendIfDefine(Defines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).AppendLine("MLogger.EndSample();");
                        sb.AppendEndIfDefine();
                    }
                    sb.AppendIndent(indent).AppendLine("}");

                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallDispose() {");
                    using (indent.Scope()) {
                        sb.AppendIfDefine(Defines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).Append("MLogger.BeginSample(\"").Append(typeName).AppendLine("_Dispose\");");
                        sb.AppendEndIfDefine();
                        
                        sb.AppendIfDefine(Defines.MORPEH_DEBUG);
                        sb.AppendIndent(indent).AppendLine("try {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("Dispose();");
                        }
                        sb.AppendIndent(indent).AppendLine("} catch (Exception exception) {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).Append("MLogger.LogError(\"Exception in ").Append(typeName).AppendLine(" system (Dispose), the system will be disabled\");");
                            sb.AppendIndent(indent).AppendLine("MLogger.LogException(exception);");
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                        sb.AppendElseDefine();
                        sb.AppendIndent(indent).AppendLine("Dispose();");
                        sb.AppendEndIfDefine();
                        
                        sb.AppendIndent(indent).AppendLine("World.Commit();");
                        
                        sb.AppendIfDefine(Defines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).AppendLine("MLogger.EndSample();");
                        sb.AppendEndIfDefine();
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                }
                
                sb.AppendIndent(indent).AppendLine("}");
                sb.AppendEndNamespace(typeDeclaration, indent);
                
                spc.AddSource($"{typeDeclaration.Identifier.Text}.system_{typeDeclaration.GetStableFileCompliantHash()}.g.cs", sb.ToString());
                
                StringBuilderPool.Return(sb);
            });
        }
    }
}