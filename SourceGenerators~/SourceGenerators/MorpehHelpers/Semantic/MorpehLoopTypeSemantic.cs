namespace SourceGenerators.MorpehHelpers.NonSemantic {
    using Microsoft.CodeAnalysis;

    public static class MorpehLoopTypeSemantic {
        public static LoopDefinition? GetLoopFromField(IFieldSymbol fieldSymbol) {
            var attributes = fieldSymbol.GetAttributes();

            for (int i = 0, length = attributes.Length; i < length; i++) {
                var attribute = attributes[i];
                
                if (attribute.AttributeClass?.Name != MorpehAttributes.LOOP_NAME) {
                    continue;
                }
                
                if (attribute.ConstructorArguments.Length == 0) {
                    continue;
                }

                var arg = attribute.ConstructorArguments[0];

                if (arg.Type is not INamedTypeSymbol loopEnumSymbol || arg.Value is not int enumNumericValue) {
                    continue;
                }

                var members = loopEnumSymbol.GetMembers();
                for (int j = 0, jlength = members.Length; j < jlength; j++) {
                    if (members[j] is not IFieldSymbol field) {
                        continue;
                    }

                    if (field.HasConstantValue && (int)field.ConstantValue == enumNumericValue) {
                        return new LoopDefinition(field.Name, enumNumericValue);
                    }
                }
            }

            return null;
        }

        public readonly struct LoopDefinition {
            public readonly string methodName;
            public readonly int index;
            
            public LoopDefinition(string methodName, int index) {
                this.methodName = methodName;
                this.index      = index;
            }
            
            public override string ToString()    => $"{this.methodName} ({this.index})";
            public override int    GetHashCode() => this.index.GetHashCode();
        }
    }
}