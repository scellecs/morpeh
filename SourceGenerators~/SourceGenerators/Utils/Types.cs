namespace SourceGenerators.Utils {
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class Types {
        public static bool HasAnyDataField(this TypeDeclarationSyntax type) {
            var members = type.Members;
            
            for (int i = 0, length = members.Count; i < length; i++) {
                if (members[i] is FieldDeclarationSyntax) {
                    return true;
                }
            }
            
            return false;
        }
    }
}