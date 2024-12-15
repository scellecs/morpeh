namespace SourceGenerators.Helpers {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Utils;

    public static class ComponentHelpers {
        public static StringBuilder AppendMetadataClassName(this StringBuilder sb, StructDeclarationSyntax structDeclaration) {
            return sb
                .Append(structDeclaration.Identifier)
                .Append("__Metadata")
                .AppendGenericParams(structDeclaration);
        }
        
        public static StringBuilder AppendStashSpecializationType(this StringBuilder sb, StructDeclarationSyntax structDeclaration) {
            return sb.Append(GetStashSpecialization(structDeclaration).type);
        }
        
        public static StashSpecialization GetStashSpecialization(StructDeclarationSyntax structDeclaration) {
            var isTag         = structDeclaration.Members.Count == 0;
            var isDisposable  = structDeclaration.BaseList?.Types.Any(t => t.Type.ToString().EndsWith("IDisposable")) ?? false;
            var componentDecl = new StringBuilder().Append(structDeclaration.Identifier).AppendGenericParams(structDeclaration).ToString();
            
            if (isTag) {
                return new StashSpecialization("TagStash", $"GetTagStash<{componentDecl}>");
            }

            if (isDisposable) {
                return new StashSpecialization($"StashD<{componentDecl}>", $"GetStashD<{componentDecl}>");
            }

            return new StashSpecialization($"Stash<{componentDecl}>", $"GetStash<{componentDecl}>");
        }
        
        public static string ComponentNameToMetadataClassName(string componentName) {
            var index = componentName.IndexOf('<');
            return index > 0 ? componentName.Insert(index, "__Metadata") : $"{componentName}__Metadata";
        }
        
        public static List<StashRequirement> GetStashRequirements(TypeDeclarationSyntax typeDeclaration) {
            var members = typeDeclaration.Members;
            var stashes = new List<StashRequirement>(members.Count);
            
            for (int i = 0, length = members.Count; i < length; i++) {
                var member = members[i];
                
                if (member is not FieldDeclarationSyntax fieldDeclaration) {
                    continue;
                }
                
                var attribute = fieldDeclaration.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .FirstOrDefault(a => a.Name.ToString() == "Require");

                if (attribute?.ArgumentList?.Arguments[0].Expression is not TypeOfExpressionSyntax typeOfExpr) {
                    continue;
                }
                
                stashes.Add(new StashRequirement {
                    fieldName     = fieldDeclaration.Declaration.Variables[0].Identifier.Text,
                    metadataClass = ComponentNameToMetadataClassName(typeOfExpr.Type.ToString()),
                });
            }

            return stashes;
        }
        
        public struct StashRequirement {
            public string  fieldName;
            public string? metadataClass;
        }
    }
}