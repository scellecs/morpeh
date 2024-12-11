namespace SourceGenerators.Utils {
    using System.Text;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class Namespaces {
        public static BaseNamespaceDeclarationSyntax? GetNamespace(this TypeDeclarationSyntax typeDeclarationSyntax) {
            var current = typeDeclarationSyntax.Parent;

            while (current != null) {
                if (current is BaseNamespaceDeclarationSyntax baseNamespaceDeclaration) {
                    return baseNamespaceDeclaration;
                }
                current = current.Parent;
            }

            return null;
        }
        
        public static bool IsGlobalNamespace(this TypeDeclarationSyntax typeDeclarationSyntax) => typeDeclarationSyntax.Parent is CompilationUnitSyntax;

        public static StringBuilder AppendNamespaceName(this StringBuilder sb, TypeDeclarationSyntax typeDeclarationSyntax) {
            var ns = GetNamespace(typeDeclarationSyntax);
            
            if (ns != null) {
                sb.Append(ns.Name);
            }
            
            return sb;
        }
        
        public static StringBuilder AppendBeginNamespace(this StringBuilder sb, TypeDeclarationSyntax typeDeclarationSyntax, int indent = 0) {
            var ns = GetNamespace(typeDeclarationSyntax);
            
            if (ns != null) {
                sb.Append(' ', indent).Append("namespace ").Append(ns.Name).Append(" {");
            }
            
            return sb;
        }
        
        public static StringBuilder AppendEndNamespace(this StringBuilder sb, TypeDeclarationSyntax typeDeclarationSyntax, int indent = 0) {
            var ns = GetNamespace(typeDeclarationSyntax);
            
            if (ns != null) {
                sb.Append(' ', indent).Append("}");
            }
            
            return sb;
        }
    }
}