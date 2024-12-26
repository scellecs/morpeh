namespace SourceGenerators.MorpehHelpers.Semantic {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NonSemantic;
    using SourceGenerators.Utils.NonSemantic;
    using Utils.Pools;

    public static class MorpehComponentHelpersSemantic {
        public static StashSpecialization GetStashSpecialization(ITypeSymbol? typeSymbol) {
            if (typeSymbol is not INamedTypeSymbol structSymbol) {
                return new StashSpecialization(StashVariation.Unknown, "Scellecs.Morpeh.Stash<?>", "Scellecs.Morpeh.GetStash<?>", "?");
            }
            
            var componentDecl = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            return GetStashSpecializationInternal(typeSymbol, componentDecl);
        }
        
        public static StashSpecialization GetStashSpecialization(SemanticModel semanticModel, StructDeclarationSyntax structDeclaration) {
            if (semanticModel.GetDeclaredSymbol(structDeclaration) is not INamedTypeSymbol structSymbol) {
                return new StashSpecialization(StashVariation.Unknown, "Scellecs.Morpeh.Stash<?>", "Scellecs.Morpeh.GetStash<?>", "?");
            }
            
            return GetStashSpecialization(structSymbol);
        }

        public static StashSpecialization GetStashSpecialization(SemanticModel semanticModel, TypeOfExpressionSyntax typeOfExpression) {
            var typeInfo      = semanticModel.GetTypeInfo(typeOfExpression.Type);
            var typeSymbol    = typeInfo.Type;
            var componentDecl = typeInfo.Type?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            return GetStashSpecializationInternal(typeSymbol, componentDecl);
        }

        private static StashSpecialization GetStashSpecializationInternal(ITypeSymbol? typeSymbol, string componentDecl) {
            if (typeSymbol is not { TypeKind: TypeKind.Struct }) {
                return new StashSpecialization(StashVariation.Unknown, "Scellecs.Morpeh.Stash<?>", "Scellecs.Morpeh.GetStash<?>", "?");
            }

            var members = typeSymbol.GetMembers();
            
            var isTag = members
                .Where(m => m.Kind is SymbolKind.Field or SymbolKind.Property)
                .All(f => f.IsStatic);
            
            var isDisposable = members
                .Where(m => m.Kind is SymbolKind.Method)
                .Any(m => m.Name == "Dispose");

            if (isTag) {
                return new StashSpecialization(StashVariation.Tag, "Scellecs.Morpeh.TagStash", $"GetTagStash<{componentDecl}>", "Scellecs.Morpeh.ITagComponent");
            }

            if (isDisposable) {
                return new StashSpecialization(StashVariation.Disposable, $"Scellecs.Morpeh.DisposableStash<{componentDecl}>", $"GetDisposableStash<{componentDecl}>", "Scellecs.Morpeh.IDisposableComponent");
            }

            return new StashSpecialization(StashVariation.Data, $"Scellecs.Morpeh.Stash<{componentDecl}>", $"GetStash<{componentDecl}>", "Scellecs.Morpeh.IDataComponent");
        }
        
        public static List<StashRequirement> GetStashRequirements(INamedTypeSymbol typeDeclaration) {
            var stashes = new List<StashRequirement>();

            var attributes = typeDeclaration.GetAttributes();
            
            for (int i = 0, length = attributes.Length; i < length; i++) {
                var attribute = attributes[i];
                    
                if (attribute.AttributeClass?.Name != MorpehAttributes.REQUIRE_NAME) {
                    continue;
                }

                var args = attribute.ConstructorArguments;
                if (args is not { Length: 1 }) {
                    continue;
                }
                
                if (args[0].Kind != TypedConstantKind.Type) {
                    continue;
                }
                
                if (args[0].Value is not INamedTypeSymbol { TypeKind: TypeKind.Struct } componentTypeSymbol) {
                    continue;
                }

                var componentTypeAttributes = componentTypeSymbol.GetAttributes();
                var isComponent = false;
                
                for (int j = 0, jlength = componentTypeAttributes.Length; j < jlength; j++) {
                    var componentTypeAttribute = componentTypeAttributes[j];

                    if (componentTypeAttribute.AttributeClass?.Name != MorpehAttributes.COMPONENT_NAME) {
                        continue;
                    }
                    
                    isComponent = true;
                }
                
                // TODO: Possibly report a diagnostic here
                if (!isComponent) {
                    continue;
                }

                string fieldName;
                string metadataClassName;
                
                if (componentTypeSymbol is { IsGenericType: true }) {
                    using (var scoped = StringBuilderPool.GetScoped()) {
                        fieldName = scoped.StringBuilder
                            .Append("_")
                            .Append(componentTypeSymbol.Name.ToCamelCase())
                            .Append("_")
                            .Append(string.Join("_", componentTypeSymbol.TypeArguments.Select(t => t.Name)))
                            .ToString();
                    }
                } else {
                    using (var scoped = StringBuilderPool.GetScoped()) {
                        fieldName = scoped.StringBuilder.Append("_").Append(componentTypeSymbol.Name.ToCamelCase()).ToString();
                    }
                }

                metadataClassName = componentTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                
                stashes.Add(new StashRequirement {
                    fieldName         = fieldName,
                    fieldTypeName     = GetStashSpecialization(componentTypeSymbol).type,
                    metadataClassName = metadataClassName,
                });
            }
            
            return stashes;
        }
        
        public readonly struct StashSpecialization {
            public readonly StashVariation variation;
            public readonly string    type;
            public readonly string    getStashMethod;
            public readonly string    constraintInterface;
            
            public StashSpecialization(StashVariation variation, string type, string getStashMethod, string constraintInterface) {
                this.variation           = variation;
                this.type                = type;
                this.getStashMethod      = getStashMethod;
                this.constraintInterface = constraintInterface;
            }
        }

        public enum StashVariation {
            Unknown,
            Tag,
            Disposable,
            Data,
        }
        
        public struct StashRequirement {
            public string  fieldName;
            public string  fieldTypeName;
            public string? metadataClassName;
        }
    }
}