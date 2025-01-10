namespace SourceGenerators.Utils.NonSemantic {
    using System;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Pools;

    public static class Types {
        public static bool IsDeclaredInsideAnotherType(this TypeDeclarationSyntax type) => type.Parent is TypeDeclarationSyntax;
        
        public static string GetStableFileCompliantHash(this TypeDeclarationSyntax type) {
            var sb = StringBuilderPool.Get();
            
            SyntaxNode? current = type;
            
            while (current is not null) {
                switch (current) {
                    case TypeDeclarationSyntax parentType:
                        sb.Append(parentType.Identifier.Text);
                        
                        if (parentType.TypeParameterList is not null) {
                            sb.Append(parentType.TypeParameterList.Parameters.Count);
                            
                            foreach (var typeParameter in parentType.TypeParameterList.Parameters) {
                                sb.Append(typeParameter.Identifier.Text);
                            }
                        }
                        
                        break;
                    case BaseNamespaceDeclarationSyntax ns:
                        sb.Append(ns.Name);
                        break;
                }
                
                current = current.Parent;
            }
            
            return sb.ToStringAndReturn().GetHashCode().ToString("X");
        }
        
        public static StringBuilder AppendVisibility(this StringBuilder sb, TypeDeclarationSyntax type) {
            var modifiers = type.Modifiers;
            
            for (int i = 0, length = modifiers.Count; i < length; i++) {
                switch (modifiers[i].Kind()) {
                    case SyntaxKind.PublicKeyword:
                        sb.Append("public");
                        break;
                    case SyntaxKind.InternalKeyword:
                        sb.Append("internal");
                        break;
                    case SyntaxKind.ProtectedKeyword:
                        sb.Append("protected");
                        break;
                    case SyntaxKind.PrivateKeyword:
                        sb.Append("private");
                        break;
                }
            }
            
            return sb;
        }
        
        public static string GetVisibility(TypeDeclarationSyntax type) {
            var modifiers = type.Modifiers;
            
            for (int i = 0, length = modifiers.Count; i < length; i++) {
                switch (modifiers[i].Kind()) {
                    case SyntaxKind.PublicKeyword:
                        return "public";
                    case SyntaxKind.InternalKeyword:
                        return "internal";
                    case SyntaxKind.ProtectedKeyword:
                        return "protected";
                    case SyntaxKind.PrivateKeyword:
                        return "private";
                }
            }
            
            return "public";
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