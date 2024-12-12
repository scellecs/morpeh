namespace SourceGenerators {
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

                var sb     = new StringBuilder();
                var indent = new IndentSource();

                sb.AppendMorpehDebugDefines();
                sb.AppendUsings(typeDeclaration).AppendLine();
                sb.AppendBeginNamespace(typeDeclaration).AppendLine();
                
                sb.AppendVisibility(typeDeclaration)
                    .Append(" partial ")
                    .AppendTypeDeclarationType(typeDeclaration)
                    .Append(' ')
                    .Append(typeName)
                    .AppendGenericParams(typeDeclaration)
                    .Append(" : ISystem ")
                    .AppendGenericConstraints(typeDeclaration)
                    .AppendLine(" {");
                
                
                using (indent.Scope()) {
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallAwake() {");
                    using (indent.Scope()) {
                        sb.AppendLine("#if MORPEH_PROFILING");
                        sb.AppendIndent(indent).Append("MLogger.BeginSample(\"").Append(typeName).AppendLine("_Awake\");");
                        sb.AppendLine("#endif");
                        sb.AppendLine("#if MORPEH_DEBUG");
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
                        sb.AppendLine("#else");
                        sb.AppendIndent(indent).AppendLine("OnAwake();");
                        sb.AppendLine("#endif");
                        sb.AppendLine("#if MORPEH_PROFILING");
                        sb.AppendIndent(indent).AppendLine("MLogger.EndSample();");
                        sb.AppendLine("#endif");
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    // TODO: IsEnabled() check.
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallUpdate(float deltaTime) {");
                    using (indent.Scope()) {
                        sb.AppendLine("#if MORPEH_PROFILING");
                        sb.AppendIndent(indent).Append("MLogger.BeginSample(\"").Append(typeName).AppendLine("_OnUpdate\");");
                        sb.AppendLine("#endif");
                        sb.AppendLine("#if MORPEH_DEBUG");
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
                        sb.AppendLine("#else");
                        sb.AppendIndent(indent).AppendLine("OnUpdate(deltaTime);");
                        sb.AppendLine("#endif");
                        sb.AppendLine("#if MORPEH_PROFILING");
                        sb.AppendIndent(indent).AppendLine("MLogger.EndSample();");
                        sb.AppendLine("#endif");
                    }
                    sb.AppendIndent(indent).AppendLine("}");

                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallDispose() {");
                    using (indent.Scope()) {
                        sb.AppendLine("#if MORPEH_PROFILING");
                        sb.AppendIndent(indent).Append("MLogger.BeginSample(\"").Append(typeName).AppendLine("_Dispose\");");
                        sb.AppendLine("#endif");
                        sb.AppendLine("#if MORPEH_DEBUG");
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
                        sb.AppendLine("#else");
                        sb.AppendIndent(indent).AppendLine("Dispose();");
                        sb.AppendLine("#endif");
                        sb.AppendLine("#if MORPEH_PROFILING");
                        sb.AppendIndent(indent).AppendLine("MLogger.EndSample();");
                        sb.AppendLine("#endif");
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                }
                
                sb.AppendLine("}");
                sb.AppendEndNamespace(typeDeclaration);
                
                spc.AddSource($"{typeDeclaration.Identifier.Text}.system_{typeDeclaration.GetStableFileCompliantHash()}.g.cs", sb.ToString());
            });
        }
    }
}