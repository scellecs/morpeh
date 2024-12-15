namespace SourceGenerators.Helpers {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Utils;

    public static class ComponentHelpers {
        private const string REQUIRE_ATTRIBUTE = "Require";
        
        public static StringBuilder AppendMetadataClassName(this StringBuilder sb, StructDeclarationSyntax structDeclaration) {
            return sb
                .Append(structDeclaration.Identifier)
                .Append("__Metadata")
                .AppendGenericParams(structDeclaration);
        }
        
        public static StashSpecialization GetStashSpecialization(SemanticModel semanticModel, StructDeclarationSyntax structDeclaration) {
            if (semanticModel.GetDeclaredSymbol(structDeclaration) is not ITypeSymbol structSymbol) {
                return new StashSpecialization("Stash<?>", "GetStash<?>");
            }

            var componentDecl = new StringBuilder()
                .Append(structDeclaration.Identifier)
                .AppendGenericParams(structDeclaration)
                .ToString();

            return GetStashSpecializationInternal(semanticModel, structSymbol, componentDecl);
        }

        public static StashSpecialization GetStashSpecialization(SemanticModel semanticModel, TypeOfExpressionSyntax typeOfExpression) {
            var typeInfo      = semanticModel.GetTypeInfo(typeOfExpression.Type);
            var typeSymbol    = typeInfo.Type;
            var componentDecl = typeOfExpression.Type.ToString();

            return GetStashSpecializationInternal(semanticModel, typeSymbol, componentDecl);
        }

        private static StashSpecialization GetStashSpecializationInternal(SemanticModel semanticModel, ITypeSymbol? typeSymbol, string componentDecl) {
            if (typeSymbol is not { TypeKind: TypeKind.Struct }) {
                return new StashSpecialization("Stash<?>", "GetStash<?>");
            }
            
            var isTag        = typeSymbol.GetMembers()
                .Where(m => m.Kind is SymbolKind.Field or SymbolKind.Property)
                .All(f => f.IsStatic);
            
            var disposable = semanticModel.Compilation.GetTypeByMetadataName("System.IDisposable");
            var isDisposable = disposable != null && typeSymbol.AllInterfaces
                .Contains(disposable);

            if (isTag) {
                return new StashSpecialization("TagStash", $"GetTagStash<{componentDecl}>");
            }

            if (isDisposable) {
                return new StashSpecialization($"StashD<{componentDecl}>", $"GetStashD<{componentDecl}>");
            }

            return new StashSpecialization($"Stash<{componentDecl}>", $"GetStash<{componentDecl}>");
        }

        
        public static string ComponentNameToMetadataClassName(string componentName) {
            var index = componentName.IndexOf('<');
            return index > 0 ? componentName.Insert(index, "__Metadata") : $"{componentName}__Metadata";
        }
        
        public static List<StashRequirement> GetStashRequirements(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel) {
            var stashes = new List<StashRequirement>();
            
            for (int i = 0, length = typeDeclaration.AttributeLists.Count; i < length; i++) {
                for (int j = 0, attributesLength = typeDeclaration.AttributeLists[i].Attributes.Count; j < attributesLength; j++) {
                    var attribute = typeDeclaration.AttributeLists[i].Attributes[j];
                    
                    if (attribute.Name.ToString() != REQUIRE_ATTRIBUTE) {
                        continue;
                    }
                    
                    var args = attribute.ArgumentList?.Arguments;
                    if (args is not { Count: 1 }) {
                        continue;
                    }
                
                    if (args.Value[0].Expression is not TypeOfExpressionSyntax typeOfExpression) {
                        continue;
                    }
                
                    var typeInfo = semanticModel.GetTypeInfo(typeOfExpression.Type);
                
                    string fieldName;
                    if (typeInfo.Type is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol) {
                        fieldName = $"_{namedTypeSymbol.Name.ToCamelCase()}_{string.Join("_", namedTypeSymbol.TypeArguments.Select(t => t.Name))}";
                    } else {
                        fieldName = $"_{typeOfExpression.Type.ToString().ToCamelCase()}";
                    }
                
                    stashes.Add(new StashRequirement {
                        fieldName         = fieldName,
                        fieldTypeName     = GetStashSpecialization(semanticModel, typeOfExpression).type,
                        metadataClassName = ComponentNameToMetadataClassName(typeOfExpression.Type.ToString()),
                    });
                }
            }
            
            return stashes;
        }
        
        public struct StashRequirement {
            public string  fieldName;
            public string  fieldTypeName;
            public string? metadataClassName;
        }
    }
}