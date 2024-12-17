namespace SourceGenerators.Generators.Injection {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    [Generator]
    public class InjectionSourceGenerator : IIncrementalGenerator {
        private const string INJECTABLE_ATTRIBUTE_NAME = "InjectableAttribute";
        
        private const string GENERIC_INJECTION_PROVIDER_ATTRIBUTE_NAME = "GenericInjectionProviderAttribute";
        
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (s, _) => s is ClassDeclarationSyntax cds && !cds.Modifiers.Any(SyntaxKind.AbstractKeyword),
                    static (ctx, _) => (declaration: (ClassDeclarationSyntax)ctx.Node, model: ctx.SemanticModel))
                .Where(static pair => pair.declaration is not null);

            var genericInjectionProviders = context.CompilationProvider.Select(static (compilation, _) => {
                var genericInjectionProviders = new List<GenericResolver>();
                WalkTree(compilation.Assembly.GlobalNamespace);
                return genericInjectionProviders;

                // TODO: There might be a faster way, but it works for now.
                void WalkTree(INamespaceSymbol namespaceSymbol)
                {
                    foreach (var member in namespaceSymbol.GetMembers())
                    {
                        switch (member)
                        {
                            case INamedTypeSymbol typeSymbol: {
                                if (typeSymbol.TypeKind != TypeKind.Class && typeSymbol.TypeKind != TypeKind.Struct) {
                                    break;
                                }
                                
                                foreach (var attribute in typeSymbol.GetAttributes()) {
                                    if (attribute.AttributeClass?.Name != GENERIC_INJECTION_PROVIDER_ATTRIBUTE_NAME) {
                                        continue;
                                    }

                                    if (attribute.ConstructorArguments.Length != 1) {
                                        continue;
                                    }
                    
                                    if (attribute.ConstructorArguments[0].Value is not INamedTypeSymbol baseType) {
                                        continue;
                                    }
                    
                                    genericInjectionProviders.Add(new GenericResolver
                                    {
                                        BaseType     = baseType,
                                        ResolverType = typeSymbol,
                                    });
                                }
                                break;
                            }

                            case INamespaceSymbol nestedNamespace: {
                                WalkTree(nestedNamespace);
                                break;
                            }
                        }
                    }
                }
            });
            
            context.RegisterSourceOutput(classes.Combine(genericInjectionProviders), static (spc, pair) =>
            {
                var ((typeDeclaration, semanticModel), genericProviders) = pair;
                
                var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
                if (typeSymbol is null) {
                    return;
                }

                var fields = TypesSemantic.GetFieldsWithAttribute(typeSymbol, INJECTABLE_ATTRIBUTE_NAME);
                
                if (fields.Count == 0) {
                    return;
                }
                
                var typeName = typeDeclaration.Identifier.ToString();
                
                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                sb.AppendUsings(typeDeclaration, indent).AppendLine();
                sb.AppendBeginNamespace(typeDeclaration, indent).AppendLine();

                sb.AppendIndent(indent)
                    .AppendVisibility(typeDeclaration)
                    .Append(" partial ")
                    .AppendTypeDeclarationType(typeDeclaration)
                    .Append(' ')
                    .Append(typeName)
                    .AppendGenericParams(typeDeclaration)
                    .Append(" : Scellecs.Morpeh.IInjectable ")
                    .AppendGenericConstraints(typeDeclaration)
                    .AppendLine(" {");

                using (indent.Scope()) {
                    sb.AppendIndent(indent).AppendLine("public void Inject(Scellecs.Morpeh.InjectionTable injectionTable) {");
                    using (indent.Scope()) {
                        for (int i = 0, length = fields.Count; i < length; i++) {
                            var field = fields[i];
                            
                            var resolved = false;
                            
                            if (field.Type is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol) {
                                var unboundType = namedTypeSymbol.ConstructUnboundGenericType();
                                
                                for (int j = 0, jlength = genericProviders.Count; j < jlength; j++) {
                                    var provider = genericProviders[j];
                                    
                                    if (!SymbolEqualityComparer.Default.Equals(unboundType, provider.BaseType)) {
                                        continue;
                                    }

                                    sb.AppendIndent(indent).Append(field.Name).Append(" = ((").Append(provider.ResolverType).Append(")injectionTable.Get(typeof(").Append(provider.ResolverType).Append("))).Provide<").Append(string.Join(", ", namedTypeSymbol.TypeArguments.Select(t => t.ToString()))).AppendLine(">();");
                                    resolved = true;
                                    break;
                                }
                            }
                            
                            if (resolved) {
                                continue;
                            }
                            
                            sb.AppendIndent(indent).Append(field.Name).Append(" = (").Append(field.Type).Append(")injectionTable.Get(typeof(").Append(field.Type).AppendLine("));");
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                }
                
                sb.AppendIndent(indent).AppendLine("}");
                sb.AppendEndNamespace(typeDeclaration, indent);
                
                spc.AddSource($"{typeDeclaration.Identifier.Text}.injection_{typeDeclaration.GetStableFileCompliantHash()}.g.cs", sb.ToString());
                
                StringBuilderPool.Return(sb);
                IndentSourcePool.Return(indent);
            });
        }

        private class GenericResolver {
            public INamedTypeSymbol? BaseType;
            public INamedTypeSymbol? ResolverType;
        }
    }
}