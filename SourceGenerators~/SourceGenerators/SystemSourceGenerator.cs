namespace SourceGenerators {
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Utils;

    [Generator]
    public class SystemSourceGenerator : IIncrementalGenerator {
        private const string ATTRIBUTE_NAME = "System";
        
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var classes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is TypeDeclarationSyntax typeDeclaration &&
                                                typeDeclaration.AttributeLists.Any(x => x.Attributes.Any(y => y?.Name.ToString() == ATTRIBUTE_NAME)),
                    transform: static (ctx, _) => (declaration: (TypeDeclarationSyntax)ctx.Node, model: ctx.SemanticModel))
                .Where(static pair => pair.declaration is not null);
            
            context.RegisterSourceOutput(classes, static (spc, pair) =>
            {
                var (typeDeclaration, semanticModel) = pair;

                var sb = new StringBuilder();
                
                sb.AppendUsings(typeDeclaration).AppendLine();
                sb.AppendBeginNamespace(typeDeclaration).AppendLine();
                
                sb.Append("public partial class ")
                    .Append(typeDeclaration.Identifier)
                    .AppendGenericParams(typeDeclaration)
                    .Append(" : ISystem ")
                    .AppendGenericConstraints(typeDeclaration)
                    .AppendLine(" {");
                
                sb.AppendLine("}");
                sb.AppendEndNamespace(typeDeclaration);
                
                spc.AddSource($"{typeDeclaration.Identifier.Text}.system_{typeDeclaration.GetStableFileCompliantHash()}.g.cs", sb.ToString());
            });
        }
    }
}