namespace SourceGenerators.Utils.Semantic {
    using System.Text;
    using Microsoft.CodeAnalysis;
    using NonSemantic;
    using Pools;

    public record ParentType(
        ParentType? Child,
        string TypeNameWithGenerics,
        string GenericConstraints,
        TypeKind TypeKind,
        Accessibility Visibility) {
        
        public static int WriteOpen(StringBuilder sb, IndentSource indent, ParentType? parentType) {
            var hierarchyDepth   = 0;
            
            var currentHierarchy = parentType;
            while (currentHierarchy != null) {
                sb.AppendIndent(indent)
                    .Append(Types.AsString(currentHierarchy.Visibility))
                    .Append(" partial ")
                    .Append(Types.AsString(currentHierarchy.TypeKind))
                    .Append(' ')
                    .Append(currentHierarchy.TypeNameWithGenerics)
                    .Append(currentHierarchy.GenericConstraints)
                    .AppendLine(" {");
                
                indent.Right();
                
                currentHierarchy = currentHierarchy.Child;
                hierarchyDepth++;
            }
            
            return hierarchyDepth;
        }
        
        public static void WriteClose(StringBuilder sb, IndentSource indent, int hierarchyDepth) {
            for (var i = 0; i < hierarchyDepth; i++) {
                indent.Left();
                sb.AppendIndent(indent).AppendLine("}");
            }
        }
        
        public static ParentType? FromTypeSymbol(INamedTypeSymbol typeSymbol) {
            ParentType? parentType = null;

            var currentSymbol = typeSymbol.ContainingType;
            
            while (currentSymbol != null && IsAllowedSymbol(currentSymbol.TypeKind)) {
                parentType = new ParentType(
                    Child: parentType,
                    TypeNameWithGenerics: StringBuilderPool.Get().Append(currentSymbol.Name).AppendGenericParams(currentSymbol).ToStringAndReturn(),
                    GenericConstraints: StringBuilderPool.Get().AppendGenericConstraints(currentSymbol).ToStringAndReturn(),
                    TypeKind: currentSymbol.TypeKind,
                    Visibility: currentSymbol.DeclaredAccessibility
                );

                currentSymbol = currentSymbol.ContainingType;
            }
            
            return parentType;
        }
        
        private static bool IsAllowedSymbol(TypeKind kind) => kind is TypeKind.Class or TypeKind.Struct;
    }
}