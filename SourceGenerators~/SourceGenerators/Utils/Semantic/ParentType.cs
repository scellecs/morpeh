namespace SourceGenerators.Utils.Semantic {
    using System.Text;
    using Microsoft.CodeAnalysis;
    using NonSemantic;
    using Pools;

    public record ParentType(
        ParentType? Child,
        string TypeNameWithGenerics,
        TypeKind TypeKind,
        Accessibility Visibility,
        bool IsStatic,
        bool IsAbstract,
        bool IsSealed) {
        
        public static int WriteOpen(StringBuilder sb, IndentSource indent, ParentType? parentType) {
            var hierarchyDepth   = 0;
            
            var currentHierarchy = parentType;
            while (currentHierarchy != null) {
                sb.AppendIndent(indent)
                    .Append(Types.AsString(currentHierarchy.Visibility))
                    .Append(' ');

                if (currentHierarchy.TypeKind == TypeKind.Class) {
                    if (currentHierarchy.IsStatic) {
                        sb.Append("static ");
                    }
                    else {
                        if (currentHierarchy.IsAbstract) {
                            sb.Append("abstract ");
                        } else if (currentHierarchy.IsSealed) {
                            sb.Append("sealed ");
                        }
                    }
                }

                sb.Append("partial ");
                    
                sb.Append(Types.AsString(currentHierarchy.TypeKind))
                    .Append(' ')
                    .Append(currentHierarchy.TypeNameWithGenerics)
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

        public static string ToProfilerMarkerName(ParentType? parentType, string typeName) {
            var profilerMarkerBuilder = StringBuilderPool.Get();
            
            var currentHierarchy = parentType;
            while (currentHierarchy != null) {
                profilerMarkerBuilder.Append(currentHierarchy.TypeNameWithGenerics).Append('.');
                currentHierarchy = currentHierarchy.Child;
            }
            
            profilerMarkerBuilder.Append(typeName);
            
            return profilerMarkerBuilder.ToStringAndReturn();
        }
        
        public static ParentType? FromTypeSymbol(INamedTypeSymbol typeSymbol) {
            ParentType? parentType = null;

            var currentSymbol = typeSymbol.ContainingType;
            
            var sb = StringBuilderPool.Get();
            
            while (currentSymbol != null && IsAllowedSymbol(currentSymbol.TypeKind)) {
                parentType = new ParentType(
                    Child: parentType,
                    TypeNameWithGenerics: sb.Clear().Append(currentSymbol.Name).AppendGenericParams(currentSymbol).AppendGenericConstraints(currentSymbol).ToString(),
                    TypeKind: currentSymbol.TypeKind,
                    Visibility: currentSymbol.DeclaredAccessibility,
                    IsStatic: currentSymbol.IsStatic,
                    IsAbstract: currentSymbol.IsAbstract,
                    IsSealed: currentSymbol.IsSealed
                );

                currentSymbol = currentSymbol.ContainingType;
            }
            
            StringBuilderPool.Return(sb);
            
            return parentType;
        }
        
        private static bool IsAllowedSymbol(TypeKind kind) => kind is TypeKind.Class or TypeKind.Struct;
    }
}