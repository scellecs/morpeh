﻿namespace SourceGenerators.MorpehHelpers.Semantic {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NonSemantic;
    using SourceGenerators.Utils.NonSemantic;
    using SourceGenerators.Utils.Semantic;
    using Utils.Pools;

    public static class MorpehComponentHelpersSemantic {
        public static StashSpecialization GetStashSpecialization(ITypeSymbol? typeSymbol) {
            if (typeSymbol is not INamedTypeSymbol structSymbol) {
                return new StashSpecialization("Stash<?>", "GetStash<?>", "?");
            }
            
            using var scoped = StringBuilderPool.GetScoped();
            
            var componentDecl = scoped.StringBuilder
                .Append(typeSymbol.Name)
                .AppendGenericParams(structSymbol)
                .ToString();
            
            return GetStashSpecializationInternal(typeSymbol, componentDecl);
        }
        
        public static StashSpecialization GetStashSpecialization(SemanticModel semanticModel, StructDeclarationSyntax structDeclaration) {
            if (semanticModel.GetDeclaredSymbol(structDeclaration) is not INamedTypeSymbol structSymbol) {
                return new StashSpecialization("Stash<?>", "GetStash<?>", "?");
            }
            
            return GetStashSpecialization(structSymbol);
        }

        public static StashSpecialization GetStashSpecialization(SemanticModel semanticModel, TypeOfExpressionSyntax typeOfExpression) {
            var typeInfo      = semanticModel.GetTypeInfo(typeOfExpression.Type);
            var typeSymbol    = typeInfo.Type;
            var componentDecl = typeOfExpression.Type.ToString();

            return GetStashSpecializationInternal(typeSymbol, componentDecl);
        }

        private static StashSpecialization GetStashSpecializationInternal(ITypeSymbol? typeSymbol, string componentDecl) {
            if (typeSymbol is not { TypeKind: TypeKind.Struct }) {
                return new StashSpecialization("Stash<?>", "GetStash<?>", "?");
            }

            var members = typeSymbol.GetMembers();
            
            var isTag = members
                .Where(m => m.Kind is SymbolKind.Field or SymbolKind.Property)
                .All(f => f.IsStatic);
            
            var isDisposable = members
                .Where(m => m.Kind is SymbolKind.Method)
                .Any(m => m.Name == "Dispose");

            if (isTag) {
                return new StashSpecialization("TagStash", $"GetTagStash<{componentDecl}>", "ITagComponent");
            }

            if (isDisposable) {
                return new StashSpecialization($"StashD<{componentDecl}>", $"GetStashD<{componentDecl}>", "IDisposableComponent");
            }

            return new StashSpecialization($"Stash<{componentDecl}>", $"GetStash<{componentDecl}>", "IDataComponent");
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

                if (args[0].Value is not INamedTypeSymbol componentTypeSymbol) {
                    continue;
                }
                
                string fieldName;
                string metadataClassName;
                
                if (componentTypeSymbol is { IsGenericType: true }) {
                    fieldName = $"_{componentTypeSymbol.Name.ToCamelCase()}_{string.Join("_", componentTypeSymbol.TypeArguments.Select(t => t.Name))}";
                    
                    using (var scoped = StringBuilderPool.GetScoped()) {
                        metadataClassName = scoped.StringBuilder.Append(componentTypeSymbol.Name).AppendGenericParams(componentTypeSymbol).ToString();
                    }
                } else {
                    fieldName = $"_{componentTypeSymbol.Name.ToCamelCase()}";
                    metadataClassName = componentTypeSymbol.Name;
                }
                
                stashes.Add(new StashRequirement {
                    fieldName         = fieldName,
                    fieldTypeName     = GetStashSpecialization(componentTypeSymbol).type,
                    metadataClassName = metadataClassName,
                });
            }
            
            return stashes;
        }
        
        public readonly struct StashSpecialization {
            public readonly string type;
            public readonly string getStashMethod;
            public readonly string constraintInterface;
            
            public StashSpecialization(string type, string getStashMethod, string constraintInterface) {
                this.type               = type;
                this.getStashMethod     = getStashMethod;
                this.constraintInterface = constraintInterface;
            }
        }
        
        public struct StashRequirement {
            public string  fieldName;
            public string  fieldTypeName;
            public string? metadataClassName;
        }
    }
}