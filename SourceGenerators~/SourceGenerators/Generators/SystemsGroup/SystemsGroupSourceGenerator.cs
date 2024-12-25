namespace SourceGenerators.Generators.SystemsGroup {
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    [Generator]
    public class SystemsGroupSourceGenerator : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.SYSTEMS_GROUP_FULL_NAME,
                (s, _) => s is TypeDeclarationSyntax,
                (ctx, _) => (ctx.TargetNode as TypeDeclarationSyntax, ctx.TargetSymbol, ctx.SemanticModel, ctx.Attributes));
            
            var disposableInterface = context.CompilationProvider
                .Select(static (compilation, _) => compilation.GetTypeByMetadataName(KnownTypes.DISPOSABLE_FULL_NAME));
            
            context.RegisterSourceOutput(classes.Combine(disposableInterface), static (spc, pair) =>
            {
                var ((typeDeclaration, typeSymbol, semanticModel, systemsGroupAttributes), disposableSymbol) = pair;
                if (typeDeclaration is null) {
                    return;
                }
                
                if (disposableSymbol is null) {
                    return;
                }
                
                using var scopedFieldDefinitionCollection = SystemsGroupFieldDefinitionCache.GetScoped();
                
                for (int i = 0, length = typeDeclaration.Members.Count; i < length; i++) {
                    if (typeDeclaration.Members[i] is not FieldDeclarationSyntax fieldDeclaration) {
                        continue;
                    }

                    if (semanticModel.GetDeclaredSymbol(fieldDeclaration.Declaration.Variables.First()) is not IFieldSymbol fieldSymbol) {
                        continue;
                    }
                    
                    var fieldAttributes = fieldSymbol.GetAttributes();
                    var typeAttributes  = fieldSymbol.Type.GetAttributes();
                    
                    var fieldDefinition = scopedFieldDefinitionCollection.Create();
                    
                    fieldDefinition.fieldDeclaration = fieldDeclaration;
                    fieldDefinition.fieldSymbol      = fieldSymbol;
                    fieldDefinition.loopType         = LoopTypeHelpers.GetLoopMethodNameFromField(fieldSymbol);
                    fieldDefinition.isSystem         = typeAttributes.Any(x => x.AttributeClass?.Name == MorpehAttributes.SYSTEM_NAME);
                    fieldDefinition.isInitializer    = typeAttributes.Any(x => x.AttributeClass?.Name == MorpehAttributes.INITIALIZER_NAME);
                    fieldDefinition.isDisposable     = fieldSymbol.Type.AllInterfaces.Contains(disposableSymbol);
                    fieldDefinition.isInjectable     = TypesSemantic.ContainsFieldsWithAttribute(fieldSymbol.Type, MorpehAttributes.INJECTABLE_NAME);
                    
                    for (int j = 0, jlength = fieldAttributes.Length; j < jlength; j++) {
                        var attribute = fieldAttributes[j];
                        
                        if (attribute.AttributeClass?.Name != MorpehAttributes.REGISTER_NAME) {
                            continue;
                        }

                        fieldDefinition.register   = true;
                        fieldDefinition.registerAs = fieldSymbol.Type as INamedTypeSymbol;

                        if (attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0] is { Kind: TypedConstantKind.Type, Value: INamedTypeSymbol injectAsPositionalSymbol }) {
                            fieldDefinition.registerAs = injectAsPositionalSymbol;
                        } else if (attribute.NamedArguments.Length > 0 && attribute.NamedArguments[0].Value is { Kind: TypedConstantKind.Type, Value: INamedTypeSymbol injectAsNamedSymbol }) {
                            fieldDefinition.registerAs = injectAsNamedSymbol;
                        }

                        break;
                    }
                    
                    scopedFieldDefinitionCollection.Add(fieldDefinition);
                }
                
                if (!RunDiagnostics(spc, semanticModel, scopedFieldDefinitionCollection)) {
                    return;
                }

                var typeName = typeDeclaration.Identifier.ToString();

                var inlineUpdateMethods = false;
                for (int i = 0, length = systemsGroupAttributes.Length; i < length; i++) {
                    var attribute = systemsGroupAttributes[i];
                    
                    var args = attribute.ConstructorArguments;
                    if (args.Length >= 1 && args[0].Value is bool inlineUpdateMethodsValue) {
                        inlineUpdateMethods = inlineUpdateMethodsValue;
                    }
                }

                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                sb.AppendUsings(typeDeclaration).AppendLine();
                sb.AppendBeginNamespace(typeDeclaration, indent).AppendLine();
                
                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent)
                    .AppendVisibility(typeDeclaration)
                    .Append(" partial ")
                    .AppendTypeDeclarationType(typeDeclaration)
                    .Append(' ')
                    .Append(typeName)
                    .AppendGenericParams(typeDeclaration)
                    .AppendGenericConstraints(typeDeclaration)
                    .AppendLine(" {");

                using (indent.Scope()) {
                    sb.AppendIndent(indent).AppendLine("private readonly Scellecs.Morpeh.InjectionTable _injectionTable;");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).Append("public ").Append(typeName).AppendLine("(Scellecs.Morpeh.World world, Scellecs.Morpeh.InjectionTable injectionTable = null) {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).AppendLine("_injectionTable = injectionTable;");
                        
                        for (int i = 0, length = scopedFieldDefinitionCollection.Collection.ordered.Count; i < length; i++) {
                            var fieldDefinition = scopedFieldDefinitionCollection.Collection.ordered[i];

                            if (fieldDefinition.isSystem || fieldDefinition.isInitializer) {
                                sb.AppendIndent(indent).AppendLine($"{fieldDefinition.fieldSymbol?.Name} = new {fieldDefinition.fieldDeclaration?.Declaration.Type}(world);");
                            } else {
                                sb.AppendIndent(indent).AppendLine($"{fieldDefinition.fieldSymbol?.Name} = new {fieldDefinition.fieldDeclaration?.Declaration.Type}();");
                            }
                        }
                        
                        sb.AppendIndent(indent).AppendLine("if (injectionTable != null) {");
                        using (indent.Scope()) {
                            for (int i = 0, length = scopedFieldDefinitionCollection.Collection.ordered.Count; i < length; i++) {
                                var fieldDefinition = scopedFieldDefinitionCollection.Collection.ordered[i];
                            
                                if (fieldDefinition.register) {
                                    if (SymbolEqualityComparer.Default.Equals(fieldDefinition.fieldSymbol?.Type, fieldDefinition.registerAs)) {
                                        sb.AppendIndent(indent).Append("injectionTable.Register(").Append(fieldDefinition.fieldSymbol?.Name).AppendLine(");");
                                    } else {
                                        sb.AppendIndent(indent).Append("injectionTable.Register(").Append(fieldDefinition.fieldSymbol?.Name).Append(", typeof(").Append(fieldDefinition.registerAs).AppendLine("));");
                                    }
                                }
                            }
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void Inject(Scellecs.Morpeh.InjectionTable injectionTable) {");
                    using (indent.Scope()) {
                        for (int i = 0, length = scopedFieldDefinitionCollection.Collection.ordered.Count; i < length; i++) {
                            var fieldDefinition = scopedFieldDefinitionCollection.Collection.ordered[i];
                            
                            if (fieldDefinition.isInjectable) {
                                sb.AppendIndent(indent).Append(fieldDefinition.fieldSymbol?.Name).AppendLine(".Inject(injectionTable);");
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallAwake() {");
                    using (indent.Scope()) {
                        for (int i = 0, length = scopedFieldDefinitionCollection.Collection.ordered.Count; i < length; i++) {
                            var fieldDefinition = scopedFieldDefinitionCollection.Collection.ordered[i];
                            
                            if (fieldDefinition.isSystem || fieldDefinition.isInitializer) {
                                sb.AppendIndent(indent).AppendLine($"{fieldDefinition.fieldSymbol?.Name}.CallAwake();");
                            }
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void CallDispose() {");
                    using (indent.Scope()) {
                        for (int i = 0, length = scopedFieldDefinitionCollection.Collection.ordered.Count; i < length; i++) {
                            var fieldDefinition = scopedFieldDefinitionCollection.Collection.ordered[i];
                            
                            if (fieldDefinition.isSystem || fieldDefinition.isInitializer) {
                                sb.AppendIndent(indent).AppendLine($"{fieldDefinition.fieldSymbol?.Name}.CallDispose();");
                            } else if (fieldDefinition.isDisposable) {
                                sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                                sb.AppendIndent(indent).AppendLine("try {");
                                sb.AppendEndIfDefine();
                                using (indent.Scope()) {
                                    sb.AppendIndent(indent).AppendLine($"{fieldDefinition.fieldSymbol?.Name}.Dispose();");
                                }
                                sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                                sb.AppendIndent(indent).AppendLine("} catch (Exception exception) {");
                                using (indent.Scope()) {
                                    sb.AppendIndent(indent).Append("MLogger.LogError(\"Exception in ").Append(typeName).AppendLine(" (Dispose)\");");
                                    sb.AppendIndent(indent).AppendLine("MLogger.LogException(exception);");
                                }
                                sb.AppendIndent(indent).AppendLine("}");
                                sb.AppendEndIfDefine();
                            }
                        }
                        
                        sb.AppendIndent(indent).AppendLine("if (_injectionTable != null) {");
                        using (indent.Scope()) {
                            for (int i = 0, length = scopedFieldDefinitionCollection.Collection.ordered.Count; i < length; i++) {
                                var fieldDefinition = scopedFieldDefinitionCollection.Collection.ordered[i];

                                if (fieldDefinition.register) {
                                    sb.AppendIndent(indent).Append("_injectionTable.UnRegister(typeof(").Append(fieldDefinition.registerAs).AppendLine("));");
                                }
                            }
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    for (int i = 0, length = LoopTypeHelpers.loopMethodNames.Length; i < length; i++) {
                        var methodName  = LoopTypeHelpers.loopMethodNames[i];
                        
                        if (!scopedFieldDefinitionCollection.Collection.byLoopType.TryGetValue(methodName, out var loopMethods) || loopMethods.Count == 0) {
                            continue;
                        }
                        
                        sb.AppendLine().AppendLine();
                        if (inlineUpdateMethods) {
                            sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                        }
                        sb.AppendIndent(indent).Append("public void ").Append(methodName).AppendLine("(float deltaTime) {");
                        using (indent.Scope()) {
                            foreach (var fieldDefinition in loopMethods) {
                                sb.AppendIndent(indent).Append(fieldDefinition.fieldSymbol?.Name).AppendLine(".CallUpdate(deltaTime);");
                            }
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                    }
                }
                
                sb.AppendIndent(indent).AppendLine("}");
                sb.AppendEndNamespace(typeDeclaration, indent);
                
                spc.AddSource($"{typeDeclaration.Identifier.Text}.systemsgroup_{typeDeclaration.GetStableFileCompliantHash()}.g.cs", sb.ToString());
                
                StringBuilderPool.Return(sb);
                IndentSourcePool.Return(indent);
            });
        }

        private static bool RunDiagnostics(SourceProductionContext ctx, SemanticModel semanticModel, ScopedSystemsGroupFieldDefinitionCollection scopedSystemsGroupFieldDefinitionCollection) {
            var success = true;
            
            for (int i = 0, length = scopedSystemsGroupFieldDefinitionCollection.Collection.ordered.Count; i < length; i++) {
                var fieldDefinition = scopedSystemsGroupFieldDefinitionCollection.Collection.ordered[i];

                if (fieldDefinition is { isSystem : true, loopType: null }) {
                    Errors.ReportMissingLoopType(ctx, fieldDefinition.fieldDeclaration);
                    success = false;
                }

                if (fieldDefinition is { isSystem: false, loopType: not null }) {
                    Errors.ReportLoopTypeOnNonSystemField(ctx, fieldDefinition.fieldDeclaration);
                    success = false;
                }
                
                if (fieldDefinition.register) {
                    if (fieldDefinition.fieldSymbol.Type.TypeKind is not TypeKind.Class) {
                        Errors.ReportInvalidInjectionSourceType(ctx, fieldDefinition.fieldDeclaration, fieldDefinition.fieldSymbol.Type.Name);
                        success = false;
                    } else {
                        var conversionKind = semanticModel.Compilation.ClassifyConversion(fieldDefinition.fieldSymbol.Type, fieldDefinition.registerAs);
                        if (conversionKind is { IsImplicit: false, IsExplicit: false }) {
                            Errors.ReportInvalidInjectionType(ctx, fieldDefinition.fieldDeclaration, fieldDefinition.fieldSymbol.Type.Name, fieldDefinition.registerAs.Name);
                            success = false;
                        }
                    }
                }
            }
            
            return success;
        }
    }
}