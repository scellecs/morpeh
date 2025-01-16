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
        
        public static SyntaxKind GetVisibilityModifier(TypeDeclarationSyntax type) {
            var modifiers = type.Modifiers;
            
            for (int i = 0, length = modifiers.Count; i < length; i++) {
                var kind = modifiers[i].Kind();
                if (kind is SyntaxKind.PublicKeyword or SyntaxKind.InternalKeyword or SyntaxKind.ProtectedKeyword or SyntaxKind.PrivateKeyword) {
                    return kind;
                }
            }
            
            return SyntaxKind.PublicKeyword;
        }
        
        public static string AsString(SyntaxKind syntaxKind) {
            switch (syntaxKind) {
                case SyntaxKind.PublicKeyword:
                    return "public";
                case SyntaxKind.InternalKeyword:
                    return "internal";
                case SyntaxKind.ProtectedKeyword:
                    return "protected";
                case SyntaxKind.PrivateKeyword:
                    return "private";
            }
            
            return "public";
        }
        
        public static string AsString(Accessibility accessibility) {
            return accessibility switch {
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                Accessibility.Protected => "protected",
                Accessibility.Private => "private",
                _ => "public"
            };
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
        
        public static TypeDeclType TypeDeclTypeFromSyntaxNode(SyntaxNode syntaxNode) {
            return syntaxNode switch {
                StructDeclarationSyntax _ => TypeDeclType.Struct,
                ClassDeclarationSyntax _ => TypeDeclType.Class,
                _ => throw new ArgumentOutOfRangeException(nameof(syntaxNode))
            };
        }
        
        public static string AsString(TypeDeclType typeDeclType) {
            return typeDeclType switch {
                TypeDeclType.Struct => "struct",
                TypeDeclType.Class  => "class",
                _ => ""
            };
        }

        public static string AsString(TypeKind typeKind) {
            return typeKind switch {
                TypeKind.Class => "class",
                TypeKind.Enum => "enum",
                TypeKind.Interface => "interface",
                TypeKind.Struct => "struct",
                _ => ""
            };
        }
    }
}