namespace SourceGenerators.Utils {
    using System.Text;
    using Helpers;
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
                sb.AppendIndent(indent).Append("namespace ").Append(ns.Name).Append(" {");
            }
            
            return sb;
        }
        
        public static StringBuilder AppendBeginNamespace(this StringBuilder sb, TypeDeclarationSyntax typeDeclarationSyntax, IndentSource indent) {
            var ns = GetNamespace(typeDeclarationSyntax);
            
            if (ns != null) {
                sb.AppendIndent(indent).Append("namespace ").Append(ns.Name).Append(" {");
                indent.Right();
            }
            
            return sb;
        }
        
        public static StringBuilder AppendEndNamespace(this StringBuilder sb, TypeDeclarationSyntax typeDeclarationSyntax, int indent = 0) {
            var ns = GetNamespace(typeDeclarationSyntax);
            
            if (ns != null) {
                sb.AppendIndent(indent).Append("}");
            }
            
            return sb;
        }
        
        public static StringBuilder AppendEndNamespace(this StringBuilder sb, TypeDeclarationSyntax typeDeclarationSyntax, IndentSource indent) {
            var ns = GetNamespace(typeDeclarationSyntax);
            
            if (ns != null) {
                indent.Left();
                sb.AppendIndent(indent).Append("}");
            }
            
            return sb;
        }

        public static StringBuilder AppendUsings(this StringBuilder sb, TypeDeclarationSyntax typeDeclarationSyntax, int indent = 0) {
            var ns = GetNamespace(typeDeclarationSyntax);

            if (ns != null) {
                foreach (var usingDirective in ns.Usings) {
                    sb.AppendIndent(indent).Append(usingDirective).AppendLine();
                }
            }

            return sb;
        }
        
        public static StringBuilder AppendUsings(this StringBuilder sb, TypeDeclarationSyntax typeDeclarationSyntax, IndentSource indent) {
            var ns = GetNamespace(typeDeclarationSyntax);

            if (ns != null) {
                foreach (var usingDirective in ns.Usings) {
                    sb.AppendIndent(indent).Append(usingDirective).AppendLine();
                }
            }

            return sb;
        }
    }
}