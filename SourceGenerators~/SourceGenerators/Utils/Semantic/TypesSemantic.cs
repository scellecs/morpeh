namespace SourceGenerators.Utils.Semantic {
    using Microsoft.CodeAnalysis;

    public class TypesSemantic {
        /// <summary>
        /// Check if the type contains fields with the specified attribute.
        /// For original type, checks all fields.
        /// For base types, checks only public and protected fields.
        /// </summary>
        public static bool ContainsFieldsWithAttribute(ITypeSymbol typeSymbol, string attributeName) {
            var includePrivate = true;
            var currentSymbol = typeSymbol;

            while (currentSymbol != null) {
                var members = currentSymbol.GetMembers();
                
                for (int i = 0, length = members.Length; i < length; i++) {
                    if (members[i] is not IFieldSymbol fieldSymbol) {
                        continue;
                    }
                    
                    if (!includePrivate && fieldSymbol.DeclaredAccessibility == Accessibility.Private) {
                        continue;
                    }

                    var attributes = fieldSymbol.GetAttributes();
                    
                    for (int j = 0, attributesLength = attributes.Length; j < attributesLength; j++) {
                        if (attributes[j].AttributeClass?.Name == attributeName) {
                            return true;
                        }
                    }
                }

                currentSymbol  = currentSymbol.BaseType;
                includePrivate = false;
            }

            return false;
        }
    }
}