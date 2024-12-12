namespace SourceGenerators.Utils {
    using System.Text;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class Types {
        public static bool IsDeclaredInsideAnotherType(this TypeDeclarationSyntax type) => type.Parent is TypeDeclarationSyntax;
        
        public static string GetStableFileCompliantHash(this TypeDeclarationSyntax type) {
            var sb = new StringBuilder();
            
            sb.Append(type.Identifier.Text);

            var parent = type.Parent;
            
            while (parent is not null) {
                switch (parent) {
                    case TypeDeclarationSyntax parentType:
                        sb.Append(parentType.Identifier.Text);
                        break;
                    case BaseNamespaceDeclarationSyntax ns:
                        sb.Append(ns.Name);
                        break;
                }
                
                parent = parent.Parent;
            }
            
            return sb.ToString().GetHashCode().ToString("X");
        }
    }
}