namespace SourceGenerators.MorpehHelpers.Semantic {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using NonSemantic;
    using SourceGenerators.Utils.NonSemantic;

    public static class MorpehComponentHelpersSemantic {
        public static StashSpecialization GetStashSpecialization(SemanticModel semanticModel, StructDeclarationSyntax structDeclaration) {
            if (semanticModel.GetDeclaredSymbol(structDeclaration) is not ITypeSymbol structSymbol) {
                return new StashSpecialization("Stash<?>", "GetStash<?>", "?");
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
                return new StashSpecialization("Stash<?>", "GetStash<?>", "?");
            }
            
            var isTag        = typeSymbol.GetMembers()
                .Where(m => m.Kind is SymbolKind.Field or SymbolKind.Property)
                .All(f => f.IsStatic);
            
            var disposable = semanticModel.Compilation.GetTypeByMetadataName("System.IDisposable");
            var isDisposable = disposable != null && typeSymbol.AllInterfaces
                .Contains(disposable);

            if (isTag) {
                return new StashSpecialization("TagStash", $"GetTagStash<{componentDecl}>", "ITagComponent");
            }

            if (isDisposable) {
                return new StashSpecialization($"StashD<{componentDecl}>", $"GetStashD<{componentDecl}>", "IDisposableComponent");
            }

            return new StashSpecialization($"Stash<{componentDecl}>", $"GetStash<{componentDecl}>", "IDataComponent");
        }
        
        public static List<StashRequirement> GetStashRequirements(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel) {
            var stashes = new List<StashRequirement>();
            
            for (int i = 0, length = typeDeclaration.AttributeLists.Count; i < length; i++) {
                for (int j = 0, attributesLength = typeDeclaration.AttributeLists[i].Attributes.Count; j < attributesLength; j++) {
                    var attribute = typeDeclaration.AttributeLists[i].Attributes[j];
                    
                    if (attribute.Name.ToString() != "Require") {
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
                        metadataClassName = typeOfExpression.Type.ToString(),
                    });
                }
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