namespace SourceGenerators.Utils {
    using System.Text;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class Generics {
        public static StringBuilder AppendGenericParams(this StringBuilder sb, TypeDeclarationSyntax typeDeclarationSyntax) {
            if (typeDeclarationSyntax.TypeParameterList is not { Parameters: { Count: > 0 } }) {
                return sb;
            }

            var parameters = string.Join(", ", typeDeclarationSyntax.TypeParameterList.Parameters);
            sb.Append($"<{parameters}>");

            return sb;
        }
        
        public static StringBuilder AppendGenericConstraints(this StringBuilder sb, TypeDeclarationSyntax typeDeclaration) {
            foreach (var constraintClause in typeDeclaration.ConstraintClauses) {
                sb.Append(" ").Append(constraintClause.ToFullString());
            }
            return sb;
        }
    }
}