namespace SourceGenerators.Utils.NonSemantic {
    using System.Text;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    public static class Generics {
        public static StringBuilder AppendGenericParams(this StringBuilder sb, TypeDeclarationSyntax typeDeclarationSyntax) {
            if (typeDeclarationSyntax.TypeParameterList is not { Parameters: { Count: > 0 } }) {
                return sb;
            }

            sb.Append("<");
            
            for (int i = 0, length = typeDeclarationSyntax.TypeParameterList.Parameters.Count; i < length; i++) {
                sb.Append(typeDeclarationSyntax.TypeParameterList.Parameters[i].Identifier.Text);
                if (i < length - 1) {
                    sb.Append(", ");
                }
            }
            
            sb.Append(">");

            return sb;
        }
        
        // TODO: Must be fully-qualified
        public static StringBuilder AppendGenericConstraints(this StringBuilder sb, TypeDeclarationSyntax typeDeclaration) {
            foreach (var constraintClause in typeDeclaration.ConstraintClauses) {
                sb.Append(" ").Append(constraintClause.ToFullString());
            }
            return sb;
        }
    }
}