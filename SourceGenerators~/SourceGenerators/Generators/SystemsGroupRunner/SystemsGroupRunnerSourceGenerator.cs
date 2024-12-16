namespace SourceGenerators.Generators.SystemsGroupRunner {
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
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

                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                sb.AppendUsings(classDeclaration).AppendLine();
                sb.AppendBeginNamespace(classDeclaration, indent).AppendLine();
                
                sb.AppendIndent(indent).Append("public partial class ").Append(classDeclaration.Identifier).Append(" {").AppendLine();

                for (int i = 0, length = classDeclaration.Members.Count; i < length; i++) {
                    if (classDeclaration.Members[i] is not FieldDeclarationSyntax fieldDeclaration) {
                        continue;
                    }
                    
                    // TODO: Generate systems group call for each field
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