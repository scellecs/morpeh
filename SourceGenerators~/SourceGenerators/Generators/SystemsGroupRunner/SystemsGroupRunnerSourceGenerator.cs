namespace SourceGenerators.Generators.SystemsGroupRunner {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using SystemsGroup;
    using Utils.NonSemantic;
    using Utils.Pools;

    [Generator]
    public class SystemsGroupRunnerSourceGenerator : IIncrementalGenerator {
        private const string ATTRIBUTE_NAME = "SystemsGroupRunner";
        
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var classes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is ClassDeclarationSyntax classDeclaration &&
                                                classDeclaration.AttributeLists.Any(x => x.Attributes.Any(y => y?.Name.ToString() == ATTRIBUTE_NAME)),
                    transform: static (ctx, _) => (declaration: (ClassDeclarationSyntax)ctx.Node, model: ctx.SemanticModel))
                .Where(static pair => pair.declaration is not null);
            
            context.RegisterSourceOutput(classes, static (spc, pair) =>
            {
                var (classDeclaration, semanticModel) = pair;

                var typeName = classDeclaration.Identifier.ToString();

                var fields = RunnerFieldDefinitionCache.GetList();
                
                for (int i = 0, length = classDeclaration.Members.Count; i < length; i++) {
                    if (classDeclaration.Members[i] is not FieldDeclarationSyntax fieldDeclaration) {
                        continue;
                    }

                    fields.Add(new RunnerFieldDefinition(
                        fieldDeclaration: fieldDeclaration, 
                        typeName: fieldDeclaration.Declaration.Type.ToString(), 
                        fieldName: fieldDeclaration.Declaration.Variables[0].Identifier.Text));
                }

                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                sb.AppendUsings(classDeclaration).AppendLine();
                sb.AppendBeginNamespace(classDeclaration, indent).AppendLine();
                
                sb.AppendIndent(indent).Append("public partial class ").Append(typeName).AppendLine(" {");

                using (indent.Scope()) {
                    sb.AppendIndent(indent).Append("private readonly World _world;").AppendLine();
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).Append("public ").Append(typeName).AppendLine("(World world) {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).AppendLine("_world = world;");
                        
                        for (int i = 0, length = fields.Count; i < length; i++) {
                            sb.AppendIndent(indent).Append(fields[i].fieldName).Append(" = ").Append("new ").Append(fields[i].typeName).AppendLine("(world);");
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    // TODO: Inject
                    
                    // TODO: OnAwake
                    
                    // TODO: Dispose
                    
                    foreach (var methodName in LoopTypeHelpers.loopMethodNames) {
                        // TODO: Check if system group has such loop

                        sb.AppendLine().AppendLine();
                        sb.AppendIndent(indent).Append("public void ").Append(methodName).AppendLine("(float deltaTime) {");

                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("_world.Commit();");
                            
                            for (int i = 0, length = fields.Count; i < length; i++) {
                                sb.AppendIndent(indent).Append(fields[i].fieldName).Append('.').Append(methodName).AppendLine("(deltaTime);");
                            }
                        }
                    
                        sb.AppendIndent(indent).AppendLine("}");
                    }
                }
                
                sb.AppendIndent(indent).AppendLine("}");
                sb.AppendEndNamespace(classDeclaration, indent);
                
                spc.AddSource($"{classDeclaration.Identifier.Text}.systemsgrouprunner_{classDeclaration.GetStableFileCompliantHash()}.g.cs", sb.ToString());
                
                StringBuilderPool.Return(sb);
                IndentSourcePool.Return(indent);
            });
        }
    }
}