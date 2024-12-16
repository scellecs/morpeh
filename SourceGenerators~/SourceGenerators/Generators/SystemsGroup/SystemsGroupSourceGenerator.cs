namespace SourceGenerators.Generators.SystemsGroup {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using Utils.NonSemantic;
    using Utils.Pools;

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
                
                if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol) {
                    return;
                }
                
                var fields = typeDeclaration.Members
                    .OfType<FieldDeclarationSyntax>()
                    .ToArray();
                
                var fieldDefinitions = new FieldDefinitionCollection();
                
                for (int i = 0, length = fields.Length; i < length; i++) {
                    var fieldDeclaration = fields[i];
                    
                    var fieldSymbol = semanticModel.GetDeclaredSymbol(fieldDeclaration.Declaration.Variables.First()) as IFieldSymbol;
                    if (fieldSymbol is null) {
                        continue;
                    }

                    var typeAttributes = fieldSymbol.Type.GetAttributes();
                    var typeInterfaces = fieldSymbol.Type.AllInterfaces;
                    
                    var fieldDefinition = new FieldDefinition {
                        fieldDeclaration = fieldDeclaration,
                        fieldSymbol      = fieldSymbol,
                        loopType         = LoopTypeHelpers.GetLoopMethodNameFromField(fieldSymbol),
                        isSystem         = typeAttributes.Any(x => x.AttributeClass?.Name == "SystemAttribute"),
                        isInitializer    = typeAttributes.Any(x => x.AttributeClass?.Name == "InitializerAttribute"),
                        isDisposable     = typeInterfaces.Contains(disposableSymbol),
                    };
                    
                    fieldDefinitions.Add(fieldDefinition);
                }
                
                if (!RunDiagnostics(spc, fieldDefinitions)) {
                    return;
                }

                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                sb.AppendUsings(typeDeclaration).AppendLine();
                sb.AppendBeginNamespace(typeDeclaration, indent).AppendLine();
                
                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent).Append("public partial class ").Append(typeDeclaration.Identifier).AppendLine(" {");
                
                /*
                using (indent.Scope()) {
                    foreach (var group in groups) {
                        GenerateUpdateMethod(sb, $"{group.Key}", group.Value, indent);
                    }
                }
                */
                
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