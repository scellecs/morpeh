namespace SourceGenerators.Utils.NonSemantic {
    using Microsoft.CodeAnalysis;

    public static class Types {
        public static string AsString(Accessibility accessibility) {
            return accessibility switch {
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                Accessibility.Protected => "protected",
                Accessibility.Private => "private",
                _ => "public"
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