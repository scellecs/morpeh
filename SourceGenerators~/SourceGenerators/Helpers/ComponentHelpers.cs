namespace SourceGenerators.Helpers {
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
                return new StashSpecialization("TagStash", $"GetTagStash<{componentDecl}>", isTag, isDisposable);
            }

            if (isDisposable) {
                return new StashSpecialization($"StashD<{componentDecl}>", $"GetStashD<{componentDecl}>", isTag, isDisposable);
            }

            return new StashSpecialization($"Stash<{componentDecl}>", $"GetStash<{componentDecl}>", isTag, isDisposable);
        }
    }
}