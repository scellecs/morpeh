namespace SourceGenerators.Utils.Semantic {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Caches;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Pools;

    public static class GenericsSemantic {
        public static StringBuilder AppendGenericParams(this StringBuilder sb, INamedTypeSymbol typeSymbol) {
            if (typeSymbol.TypeArguments.Length == 0) {
                return sb;
            }
            
            sb.Append("<");
            
            for (int i = 0, length = typeSymbol.TypeArguments.Length; i < length; i++) {
                sb.Append(typeSymbol.TypeArguments[i].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                if (i < length - 1) {
                    sb.Append(", ");
                }
            }
            
            sb.Append(">");

            return sb;
        }

        public static StringBuilder AppendGenericConstraints(this StringBuilder sb, INamedTypeSymbol originSymbol) {
            var typeParameters = originSymbol.TypeParameters;
            
            if (typeParameters.Length == 0) {
                return sb;
            }

            var sections = ThreadStaticListCache<string>.GetClear();

            for (int i = 0, length = typeParameters.Length; i < length; i++) {
                var typeParam = typeParameters[i];
                
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

                var constraintTypes = typeParam.ConstraintTypes;
                for (int j = 0, jlength = constraintTypes.Length; j < jlength; j++) {
                    sections.Add(constraintTypes[j].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                }

                if (typeParam.HasConstructorConstraint) {
                    sections.Add("new()");
                }

                for (int j = 0, jlength = sections.Count; j < jlength; j++) {
                    sb.Append(sections[j]);
                    if (j < jlength - 1) {
                        sb.Append(", ");
                    }
                }
            }

            return sb;
        }

        public static (string, string) GetGenericParamsAndConstraints(INamedTypeSymbol symbol) {
            string genericParams;
            string genericConstraints;
            
            if (symbol.TypeParameters.Length > 0) {
                var sb = StringBuilderPool.Get();
                
                genericParams = sb
                    .AppendGenericParams(symbol)
                    .ToString();
                
                genericConstraints = sb
                    .Clear()
                    .AppendGenericConstraints(symbol)
                    .ToStringAndReturn();
                
            } else {
                genericParams      = string.Empty;
                genericConstraints = string.Empty;
            }
            
            return (genericParams, genericConstraints);
        }
    }
}