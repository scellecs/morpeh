namespace SourceGenerators.Generators.SystemsGroup {
    using System.Linq;
    using Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    [Generator]
    public class SystemsGroupSourceGenerator : IIncrementalGenerator {
        private const string ATTRIBUTE_NAME = "SystemsGroup";
        private const string DISPOSABLE_INTERFACE_NAME = "System.IDisposable";
        
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var classes = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is TypeDeclarationSyntax typeDeclaration &&
                                                typeDeclaration.AttributeLists.Any(x => x.Attributes.Any(y => y?.Name.ToString() == ATTRIBUTE_NAME)),
                    transform: static (ctx, _) => (declaration: (TypeDeclarationSyntax)ctx.Node, model: ctx.SemanticModel))
                .Where(static pair => pair.declaration is not null);
            
            var disposableInterface = context.CompilationProvider
                .Select(static (compilation, _) => compilation.GetTypeByMetadataName(DISPOSABLE_INTERFACE_NAME));
            
            context.RegisterSourceOutput(classes.Combine(disposableInterface), static (spc, pair) =>
            {
                var ((typeDeclaration, semanticModel), disposableSymbol) = pair;
                
                if (disposableSymbol is null) {
                    return;
                }
                
                var fields = typeDeclaration.Members
                    .OfType<FieldDeclarationSyntax>()
                    .ToArray();
                
                // TODO: Store as a thread-static to avoid reallocation
                var fieldDefinitions = new FieldDefinitionCollection();
                
                for (int i = 0, length = fields.Length; i < length; i++) {
                    var fieldDeclaration = fields[i];
                    
                    var fieldSymbol = semanticModel.GetDeclaredSymbol(fieldDeclaration.Declaration.Variables.First()) as IFieldSymbol;
                    if (fieldSymbol is null) {
                        continue;
                    }

                    var typeAttributes = fieldSymbol.Type.GetAttributes();
                    
                    // TODO: Use thread-static pool?
                    // TODO: Move constants
                    var fieldDefinition = new FieldDefinition {
                        fieldDeclaration = fieldDeclaration,
                        fieldSymbol      = fieldSymbol,
                        loopType         = LoopTypeHelpers.GetLoopMethodNameFromField(fieldSymbol),
                        isSystem         = typeAttributes.Any(x => x.AttributeClass?.Name == "SystemAttribute"),
                        isInitializer    = typeAttributes.Any(x => x.AttributeClass?.Name == "InitializerAttribute"),
                        isDisposable     = fieldSymbol.Type.AllInterfaces.Contains(disposableSymbol),
                        isInjectable     = TypesSemantic.ContainsFieldsWithAttribute(fieldSymbol.Type, "InjectableAttribute"),
                    };
                    
                    fieldDefinitions.Add(fieldDefinition);
                }
                
                if (!RunDiagnostics(spc, fieldDefinitions)) {
                    return;
                }

                var typeName = typeDeclaration.Identifier.ToString();

                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                sb.AppendUsings(typeDeclaration).AppendLine();
                sb.AppendBeginNamespace(typeDeclaration, indent).AppendLine();
                
                // TODO: Could be something than public and class? Maybe we should throw a diagnostic if it's otherwise
                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent).Append("public partial class ").Append(typeName).AppendLine(" {");

                using (indent.Scope()) {
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).Append("public ").Append(typeName).AppendLine("(World world) {");
                    using (indent.Scope()) {
                        for (int i = 0, length = fieldDefinitions.ordered.Count; i < length; i++) {
                            var fieldDefinition = fieldDefinitions.ordered[i];
                            
                            // TODO: Inplace register before awake calls
                            sb.AppendIndent(indent).AppendLine($"{fieldDefinition.fieldSymbol.Name} = new {fieldDefinition.fieldDeclaration.Declaration.Type}();");
                            if (fieldDefinition.isSystem || fieldDefinition.isInitializer) {
                                // TODO: Possibly move to constructor
                                sb.AppendIndent(indent).Append($"{fieldDefinition.fieldSymbol.Name}.World = world;").AppendLine();
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    // TODO: Possibly move to constructor?
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void SetupRequirements() {");
                    using (indent.Scope()) {
                        for (int i = 0, length = fieldDefinitions.ordered.Count; i < length; i++) {
                            var fieldDefinition = fieldDefinitions.ordered[i];
                            
                            if (fieldDefinition.isSystem || fieldDefinition.isInitializer) {
                                sb.AppendIndent(indent).AppendLine($"{fieldDefinition.fieldSymbol.Name}.SetupRequirements();");
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    // TODO: Implement Inject generation in a separate sourcegen
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void Inject(Scellecs.Morpeh.InjectionTable injectionTable) {");
                    using (indent.Scope()) {
                        for (int i = 0, length = fieldDefinitions.ordered.Count; i < length; i++) {
                            var fieldDefinition = fieldDefinitions.ordered[i];
                            
                            if (fieldDefinition.isInjectable) {
                                sb.AppendIndent(indent).AppendLine($"injectionTable.Inject({fieldDefinition.fieldSymbol.Name});");
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallAwake() {");
                    using (indent.Scope()) {
                        for (int i = 0, length = fieldDefinitions.ordered.Count; i < length; i++) {
                            var fieldDefinition = fieldDefinitions.ordered[i];
                            
                            if (fieldDefinition.isSystem || fieldDefinition.isInitializer) {
                                sb.AppendIndent(indent).AppendLine($"{fieldDefinition.fieldSymbol.Name}.CallAwake();");
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallDispose() {");
                    using (indent.Scope()) {
                        for (int i = 0, length = fieldDefinitions.ordered.Count; i < length; i++) {
                            var fieldDefinition = fieldDefinitions.ordered[i];
                            
                            if (fieldDefinition.isSystem || fieldDefinition.isInitializer) {
                                sb.AppendIndent(indent).AppendLine($"{fieldDefinition.fieldSymbol.Name}.CallDispose();");
                            } else if (fieldDefinition.isDisposable) {
                                // TODO: Should we wrap it into a try-catch block for MORPEH_DEBUG?
                                sb.AppendIndent(indent).AppendLine($"{fieldDefinition.fieldSymbol.Name}.Dispose();");
                            }
                        }
                        
                        // TODO: Remove registrations if they were registered in constructor
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    // TODO: Update loops
                }
                
                sb.AppendIndent(indent).AppendLine("}");
                sb.AppendEndNamespace(typeDeclaration, indent);
                
                spc.AddSource($"{typeDeclaration.Identifier.Text}.systemsgroup_{typeDeclaration.GetStableFileCompliantHash()}.g.cs", sb.ToString());
                
                StringBuilderPool.Return(sb);
                IndentSourcePool.Return(indent);
            });
        }

        private static bool RunDiagnostics(SourceProductionContext ctx, FieldDefinitionCollection fieldDefinitionCollection) {
            var success = true;
            
            for (int i = 0, length = fieldDefinitionCollection.ordered.Count; i < length; i++) {
                var fieldDefinition = fieldDefinitionCollection.ordered[i];

                if (fieldDefinition.isSystem && fieldDefinition.loopType is null) {
                    Errors.ReportMissingLoopType(ctx, fieldDefinition.fieldDeclaration);
                    success = false;
                }
            }
            
            return success;
        }
    }
}