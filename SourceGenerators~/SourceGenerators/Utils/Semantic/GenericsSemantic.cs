namespace SourceGenerators.Utils.Semantic {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class GenericsSemantic {
        public static StringBuilder AppendGenericParams(this StringBuilder sb, INamedTypeSymbol typeSymbol) {
            if (typeSymbol.TypeArguments.Length == 0) {
                return sb;
            }
            
            sb.Append("<");
            
            for (int i = 0, length = typeSymbol.TypeArguments.Length; i < length; i++) {
                sb.Append(typeSymbol.TypeArguments[i].Name);
                if (i < length - 1) {
                    sb.Append(", ");
                }
            }
            
            sb.Append(">");

            return sb;
        }

        public static StringBuilder AppendGenericConstraints(this StringBuilder sb, INamedTypeSymbol originSymbol) {
            if (originSymbol.TypeParameters.Length == 0) {
                return sb;
            }

            var sections = new List<string>();

            for (int i = 0, length = originSymbol.TypeParameters.Length; i < length; i++) {
                var typeParam = originSymbol.TypeParameters[i];
                
                var hasConstraints =
                    typeParam.HasConstructorConstraint ||
                    typeParam.HasReferenceTypeConstraint ||
                    typeParam.HasNotNullConstraint ||
                    typeParam.HasValueTypeConstraint ||
                    typeParam.HasUnmanagedTypeConstraint ||
                    typeParam.ConstraintTypes.Length > 0;

                if (!hasConstraints) {
                    continue;
                }
                
                sections.Clear();

                sb.Append("where ")
                    .Append(typeParam.Name)
                    .Append(" : ");
                
                if (typeParam.HasReferenceTypeConstraint) {
                    sections.Add("class");
                } else if (typeParam.HasUnmanagedTypeConstraint) {
                    sections.Add("unmanaged");
                } else if (typeParam.HasValueTypeConstraint) {
                    sections.Add("struct");
                }

                if (typeParam.HasNotNullConstraint) {
                    sections.Add("notnull");
                }

                for (int j = 0, jlength = typeParam.ConstraintTypes.Length; j < jlength; j++) {
                    sections.Add(typeParam.ConstraintTypes[j].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }

                if (typeParam.HasConstructorConstraint) {
                    sections.Add("new()");
                }

                sb.Append(string.Join(", ", sections));
            }

            return sb;
        }
        
        // TODO: Might be useful for later but it requires full semantic model.
        /*
        public static StringBuilder AppendGenericConstraints(this StringBuilder sb, TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
        {
            for (int i = 0, length = typeDeclaration.ConstraintClauses.Count; i < length; i++) {
                var constraintClause = typeDeclaration.ConstraintClauses[i];
                
                sb.Append(" where ").Append(constraintClause.Name.Identifier.Text).Append(" : ");

                for (int j = 0, jlength = constraintClause.Constraints.Count; j < jlength; j++) {
                    var constraint = constraintClause.Constraints[j];

                    if (constraint is not TypeConstraintSyntax typeConstraint) {
                        return sb.Append(constraint);
                    }

                    var symbolInfo = semanticModel.GetSymbolInfo(typeConstraint.Type);
                    if (symbolInfo.Symbol is ITypeSymbol typeSymbol) {
                        sb.Append(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                    }

                    sb.Append(typeConstraint.Type);
                }
            }

            return sb;
        }
        */
    }
}