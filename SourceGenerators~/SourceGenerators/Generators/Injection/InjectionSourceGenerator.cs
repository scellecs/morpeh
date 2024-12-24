namespace SourceGenerators.Generators.Injection {
    using System.Linq;
    using Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    [Generator]
    public class InjectionSourceGenerator : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (s, _) => s is ClassDeclarationSyntax cds && !cds.Modifiers.Any(SyntaxKind.AbstractKeyword),
                    static (ctx, _) => (declaration: (ClassDeclarationSyntax)ctx.Node, model: ctx.SemanticModel))
                .Where(static pair => pair.declaration is not null);

            var genericResolvers = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    MorpehAttributes.GENERIC_INJECTION_RESOLVER_ATTRIBUTE_FULL_NAME,
                    static (node, _) => node is ClassDeclarationSyntax,
                    static (ctx, _) => 
                    {
                        var typeSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
                        var attribute  = ctx.Attributes.FirstOrDefault();
                        if (attribute is null) {
                            return null;
                        }
                        
                        var args = attribute.ConstructorArguments;
                        if (args.Length != 1 || args[0].Value is not INamedTypeSymbol baseType) {
                            return null;
                        }
                        
                        DiagnosticDescriptor? diagnosticDescriptor = null;
                        
                        ISymbol? provideMethod = null;
                        foreach (var member in typeSymbol.GetMembers()) {
                            if (member is not IMethodSymbol methodSymbol) {
                                continue;
                            }

                            if (methodSymbol.Name != "Resolve") {
                                continue;
                            }

                            if (!methodSymbol.IsGenericMethod) {
                                continue;
                            }

                            if (methodSymbol.TypeArguments.Length != baseType.TypeArguments.Length) {
                                continue;
                            }

                            provideMethod = methodSymbol;
                            break;
                        }
                        
                        if (provideMethod is null) {
                            diagnosticDescriptor = Errors.GENERIC_RESOLVER_HAS_NO_MATCHING_METHOD;
                        }

                        return new GenericResolver
                        {
                            BaseType     = baseType,
                            ResolverType = typeSymbol,
                            Declaration = (ClassDeclarationSyntax)ctx.TargetNode,
                            DiagnosticDescriptor = diagnosticDescriptor,
                        };
                    }
                )
                .Where(static resolver => resolver is not null)
                .Select(static (resolver, _) => resolver!);
            
            context.RegisterSourceOutput(genericResolvers.Where(x => x.DiagnosticDescriptor != null).Collect(), static (spc, pair) => {
                foreach (var resolver in pair) {
                    Errors.ReportGenericResolverIssue(spc, resolver.Declaration!, resolver.DiagnosticDescriptor!);
                }
            });
            
            context.RegisterSourceOutput(classes.Combine(genericResolvers.Where(x => x.DiagnosticDescriptor == null).Collect()), static (spc, pair) =>
            {
                var ((typeDeclaration, semanticModel), genericProviders) = pair;
                
                var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
                if (typeSymbol is null) {
                    return;
                }

                var fields = TypesSemantic.GetFieldsWithAttribute(typeSymbol, MorpehAttributes.INJECTABLE_NAME);
                
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
                                
                                for (int j = 0, jlength = genericProviders.Length; j < jlength; j++) {
                                    var provider = genericProviders[j];
                                    
                                    if (!SymbolEqualityComparer.Default.Equals(unboundType, provider.BaseType)) {
                                        continue;
                                    }

                                    sb.AppendIndent(indent).Append(field.Name).Append(" = ((").Append(provider.ResolverType).Append(")injectionTable.Get(typeof(").Append(provider.ResolverType).Append("))).Resolve<").Append(string.Join(", ", namedTypeSymbol.TypeArguments.Select(t => t.ToString()))).AppendLine(">();");
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
            
            public TypeDeclarationSyntax? Declaration;
            public DiagnosticDescriptor? DiagnosticDescriptor;
        }
    }
}