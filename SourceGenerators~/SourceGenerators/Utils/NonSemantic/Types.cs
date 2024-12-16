namespace SourceGenerators.Utils.NonSemantic {
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
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
        
        public static StringBuilder AppendVisibility(this StringBuilder sb, TypeDeclarationSyntax type) {
            if (type.Modifiers.Any(x => x.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword))) {
                sb.Append("public");
            } else if (type.Modifiers.Any(x => x.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.InternalKeyword))) {
                sb.Append("internal");
            } else if (type.Modifiers.Any(x => x.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword))) {
                sb.Append("protected");
            } else if (type.Modifiers.Any(x => x.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword))) {
                sb.Append("private");
            }
            
            return sb;
        }
        
        public static StringBuilder AppendTypeDeclarationType(this StringBuilder sb, TypeDeclarationSyntax type) {
            switch (type) {
                case ClassDeclarationSyntax _:
                    sb.Append("class");
                    break;
                case StructDeclarationSyntax _:
                    sb.Append("struct");
                    break;
                case InterfaceDeclarationSyntax _:
                    sb.Append("interface");
                    break;
            }
            
            return sb;
        }
    }
}