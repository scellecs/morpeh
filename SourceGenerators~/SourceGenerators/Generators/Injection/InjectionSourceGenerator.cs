namespace SourceGenerators.Generators.Injection {
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;
    
#if MORPEH_SOURCEGEN_INJECTABLE_SCAN_SLOW
    using Microsoft.CodeAnalysis.CSharp;
#endif

    [Generator]
    public class InjectionSourceGenerator : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
#if MORPEH_SOURCEGEN_INJECTABLE_SCAN_SLOW
            var classes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (s, _) => s is ClassDeclarationSyntax cds && !cds.Modifiers.Any(SyntaxKind.AbstractKeyword),
                    static (ctx, _) => (declaration: (ClassDeclarationSyntax)ctx.Node, model: ctx.SemanticModel))
                .Where(static pair => pair.declaration is not null);
#else
            var classes = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    MorpehAttributes.INJECTABLE_FULL_NAME,
                    static (node, _) => node is ClassDeclarationSyntax,
                    static (ctx, _) => (declaration: (ClassDeclarationSyntax)ctx.TargetNode, ctx.TargetSymbol as INamedTypeSymbol)
                )
                .Where(static pair => pair.declaration is not null);
#endif

            var genericResolvers = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    MorpehAttributes.GENERIC_INJECTION_RESOLVER_ATTRIBUTE_FULL_NAME,
                    static (node, _) => node is ClassDeclarationSyntax,
                    static (ctx, _) => {
                        var typeSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
                        
                        var attribute  = ctx.Attributes.FirstOrDefault();
                        if (attribute is null) {
                            return null;
                        }
                        
                        var args = attribute.ConstructorArguments;
                        if (args.Length != 1 || args[0].Value is not INamedTypeSymbol baseType) {
                            return null;
                        }

                        return new GenericResolver(baseType, typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                    }
                )
                .Where(static resolver => resolver is not null)
                .Select(static (resolver, _) => resolver!)
                .Collect();
            
            context.RegisterSourceOutput(classes.Combine(genericResolvers), static (spc, pair) => {
#if MORPEH_SOURCEGEN_INJECTABLE_SCAN_SLOW
                var ((typeDeclaration, semanticModel), genericProviders) = pair;
                
                if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol) {
                    return;
                }
#else
                var ((typeDeclaration, typeSymbol), genericProviders) = pair;
                
                if (typeSymbol is null) {
                    return;
                }
#endif

                // TODO: Thread static cache
                var fields = new List<IFieldSymbol>();
                TypesSemantic.FillFieldsWithAttribute(fields, typeSymbol, MorpehAttributes.INJECT_NAME);
                
                if (fields.Count == 0) {
                    return;
                }
                
                var typeName = typeDeclaration.Identifier.ToString();
                
                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                sb.AppendBeginNamespace(typeDeclaration, indent).AppendLine();

                sb.AppendIndent(indent)
                    .AppendVisibility(typeDeclaration)
                    .Append(" partial ")
                    .AppendTypeDeclarationType(typeDeclaration)
                    .Append(' ')
                    .Append(typeName)
                    .AppendGenericParams(typeDeclaration)
                    .Append(" : Scellecs.Morpeh.IInjectable ")
                    .AppendGenericConstraints(typeSymbol)
                    .AppendLine(" {");

                using (indent.Scope()) {
                    sb.AppendIndent(indent).AppendLine("public void Inject(Scellecs.Morpeh.InjectionTable injectionTable) {");
                    using (indent.Scope()) {
                        for (int i = 0, length = fields.Count; i < length; i++) {
                            var field = fields[i];
                            
                            var resolved = false;
                            
                            if (field.Type is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol) {
                                var unboundType = namedTypeSymbol.ConstructUnboundGenericType();
                                
                                for (int j = 0, jlength = genericProviders.Length; j < jlength; j++) {
                                    var provider = genericProviders[j];
                                    
                                    if (!SymbolEqualityComparer.Default.Equals(unboundType, provider.baseType)) {
                                        continue;
                                    }

                                    sb.AppendIndent(indent).Append(field.Name).Append(" = ((").Append(provider.resolverTypeName).Append(")injectionTable.Get(typeof(").Append(provider.resolverTypeName).Append("))).Resolve<").Append(string.Join(", ", namedTypeSymbol.TypeArguments.Select(static t => t.ToString()))).AppendLine(">();");
                                    resolved = true;
                                    break;
                                }
                            }
                            
                            if (resolved) {
                                continue;
                            }

                            var fieldTypeName = field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            sb.AppendIndent(indent).Append(field.Name).Append(" = (").Append(fieldTypeName).Append(")injectionTable.Get(typeof(").Append(fieldTypeName).AppendLine("));");
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
            public readonly INamedTypeSymbol baseType;
            public readonly string           resolverTypeName;

            public GenericResolver(INamedTypeSymbol baseType, string resolverTypeName) {
                this.baseType         = baseType;
                this.resolverTypeName = resolverTypeName;
            }
        }
    }
}