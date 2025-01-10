namespace SourceGenerators.MorpehHelpers.Semantic {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using NonSemantic;
    using SourceGenerators.Utils.NonSemantic;
    using Utils.Pools;

    public static class MorpehComponentHelpersSemantic {
        public static StashSpecialization GetStashSpecialization(ITypeSymbol? typeSymbol, string componentDecl) {
            if (typeSymbol is not { TypeKind: TypeKind.Struct }) {
                return new StashSpecialization("Scellecs.Morpeh.Stash<?>", "Scellecs.Morpeh.GetStash<?>", "?");
            }
            
            return GetStashSpecialization(GetStashVariation(typeSymbol), componentDecl);
        }
        
        public static StashSpecialization GetStashSpecialization(StashVariation variation, string componentDecl) {
            return variation switch {
                StashVariation.Tag => new StashSpecialization(
                    type: "Scellecs.Morpeh.TagStash",
                    getStashMethod: StringBuilderPool.Get().Append("GetTagStash<").Append(componentDecl).Append(">").ToStringAndReturn(),
                    constraintInterface: "Scellecs.Morpeh.ITagComponent"),
                StashVariation.Disposable => new StashSpecialization(
                    type: StringBuilderPool.Get().Append("Scellecs.Morpeh.DisposableStash<").Append(componentDecl).Append(">").ToStringAndReturn(),
                    getStashMethod: StringBuilderPool.Get().Append("GetDisposableStash<").Append(componentDecl).Append(">").ToStringAndReturn(),
                    constraintInterface: "Scellecs.Morpeh.IDisposableComponent"),
                _ => new StashSpecialization(
                    type: StringBuilderPool.Get().Append("Scellecs.Morpeh.Stash<").Append(componentDecl).Append(">").ToStringAndReturn(),
                    getStashMethod: StringBuilderPool.Get().Append("GetStash<").Append(componentDecl).Append(">").ToStringAndReturn(),
                    constraintInterface: "Scellecs.Morpeh.IDataComponent")
            };
        }
        
        public static StashVariation GetStashVariation(ITypeSymbol? typeSymbol) {
            if (typeSymbol is not { TypeKind: TypeKind.Struct }) {
                return StashVariation.Unknown;
            }

#if !MORPEH_SOURCEGEN_NO_STASH_SPECIALIZATION
            var members = typeSymbol.GetMembers();
            
            var isTag = members
                .Where(m => m.Kind is SymbolKind.Field or SymbolKind.Property)
                .All(f => f.IsStatic);
            
            var isDisposable = members
                .Where(m => m.Kind is SymbolKind.Method)
                .Any(m => m.Name == "Dispose" && m is IMethodSymbol { Parameters: { Length: 0 } });

            if (isTag) {
                return StashVariation.Tag;
            }

            if (isDisposable) {
                return StashVariation.Disposable;
            }
#endif

            return StashVariation.Data;
        }
        
        public static void FillStashRequirements(List<StashRequirement> stashes, INamedTypeSymbol typeDeclaration) {
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

                string fieldName;

                if (componentTypeSymbol is { IsGenericType: true }) {
                    fieldName = StringBuilderPool.Get()
                        .Append('_')
                        .Append(componentTypeSymbol.Name.ToCamelCase())
                        .Append('_')
                        .Append(string.Join("_", componentTypeSymbol.TypeArguments.Select(t => t.Name)))
                        .ToStringAndReturn();
                } else {
                    fieldName = StringBuilderPool.Get()
                        .Append('_')
                        .Append(componentTypeSymbol.Name.ToCamelCase())
                        .ToStringAndReturn();
                }

                var metadataClassName = componentTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                
                stashes.Add(new StashRequirement {
                    fieldName         = fieldName,
                    fieldTypeName     = GetStashSpecialization(componentTypeSymbol, componentDecl: metadataClassName).type,
                    metadataClassName = metadataClassName,
                });
            }
        }
        
        public struct StashRequirement {
            public string  fieldName;
            public string  fieldTypeName;
            public string? metadataClassName;
        }
    }
}