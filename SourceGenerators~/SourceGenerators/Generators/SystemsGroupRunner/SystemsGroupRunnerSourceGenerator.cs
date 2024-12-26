namespace SourceGenerators.Generators.SystemsGroupRunner {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using SystemsGroup;
    using Utils.NonSemantic;
    using Utils.Pools;

    [Generator]
    public class SystemsGroupRunnerSourceGenerator : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.SYSTEMS_GROUP_RUNNER_FULL_NAME,
                (s, _) => s is ClassDeclarationSyntax,
                (ctx, _) => (ctx.TargetNode as ClassDeclarationSyntax, ctx.SemanticModel));
            
            context.RegisterSourceOutput(classes, static (spc, pair) => {
                var (typeDeclaration, semanticModel) = pair;
                if (typeDeclaration is null) {
                    return;
                }

                var typeName = typeDeclaration.Identifier.ToString();

                var fields = RunnerFieldDefinitionCache.GetList();
                
                var existingLoops = new HashSet<string>();
                
                for (int i = 0, length = typeDeclaration.Members.Count; i < length; i++) {
                    if (typeDeclaration.Members[i] is not FieldDeclarationSyntax fieldDeclaration) {
                        continue;
                    }

                    if (semanticModel.GetSymbolInfo(fieldDeclaration.Declaration.Type).Symbol is not ITypeSymbol typeSymbol) {
                        continue;
                    }

                    var loops = new HashSet<string>();
                    
                    var members = typeSymbol.GetMembers();
                    for (int j = 0, jlength = members.Length; j < jlength; j++) {
                        if (members[j] is not IFieldSymbol fieldSymbol) {
                            continue;
                        }

                        var loopType = LoopTypeHelpers.GetLoopMethodNameFromField(fieldSymbol);
                        if (loopType == null) {
                            continue;
                        }

                        loops.Add(LoopTypeHelpers.loopMethodNames[(int)loopType]);
                    }

                    fields.Add(new RunnerFieldDefinition(
                        typeName: typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        fieldName: fieldDeclaration.Declaration.Variables[0].Identifier.Text,
                        loops: loops));
                    
                    existingLoops.UnionWith(loops);
                }

                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                sb.AppendIndent(indent).AppendLine("using Scellecs.Morpeh;");
                sb.AppendBeginNamespace(typeDeclaration, indent).AppendLine();
                
                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent)
                    .AppendVisibility(typeDeclaration)
                    .Append(" partial ")
                    .AppendTypeDeclarationType(typeDeclaration)
                    .Append(' ')
                    .Append(typeName)
                    .AppendGenericParams(typeDeclaration)
                    .Append(" : ")
                    .Append(KnownTypes.DISPOSABLE_FULL_NAME)
                    .Append(' ')
                    .AppendGenericConstraints(typeDeclaration)
                    .AppendLine(" {");

                using (indent.Scope()) {
                    sb.AppendIndent(indent).AppendLine("private readonly Scellecs.Morpeh.World _world;");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).Append("public ").Append(typeName).AppendLine("(Scellecs.Morpeh.World world, Scellecs.Morpeh.InjectionTable injectionTable = null) {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).AppendLine("_world = world;");
                        
                        for (int i = 0, length = fields.Count; i < length; i++) {
                            sb.AppendIndent(indent).Append(fields[i].fieldName).Append(" = ").Append("new ").Append(fields[i].typeName).AppendLine("(world, injectionTable);");
                        }
                        
                        sb.AppendIndent(indent).AppendLine("if (injectionTable != null) {");
                        using (indent.Scope()) {
                            for (int i = 0, length = fields.Count; i < length; i++) {
                                sb.AppendIndent(indent).Append(fields[i].fieldName).AppendLine(".Inject(injectionTable);");
                            }
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void OnAwake() {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).AppendLine("_world.Commit();");
                        
                        for (int i = 0, length = fields.Count; i < length; i++) {
                            sb.AppendIndent(indent).Append(fields[i].fieldName).AppendLine(".CallAwake();");
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void Dispose() {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).AppendLine("_world.Commit();");
                        
                        for (int i = 0, length = fields.Count; i < length; i++) {
                            sb.AppendIndent(indent).Append(fields[i].fieldName).AppendLine(".CallDispose();");
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    for (int i = 0, length = LoopTypeHelpers.loopMethodNames.Length; i < length; i++) {
                        var methodName = LoopTypeHelpers.loopMethodNames[i];
                        
                        if (!existingLoops.Contains(methodName)) {
                            continue;
                        }

                        sb.AppendLine().AppendLine();
                        sb.AppendIndent(indent).Append("public void ").Append(methodName).AppendLine("(float deltaTime) {");

                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("_world.Commit();");
                            
                            for (int j = 0, jlength = fields.Count; j < jlength; j++) {
                                if (!fields[j].loops.Contains(methodName)) {
                                    continue;
                                }
                                
                                sb.AppendIndent(indent).Append(fields[j].fieldName).Append('.').Append(methodName).AppendLine("(deltaTime);");
                            }
                        }
                    
                        sb.AppendIndent(indent).AppendLine("}");
                    }
                }
                
                sb.AppendIndent(indent).AppendLine("}");
                sb.AppendEndNamespace(typeDeclaration, indent);
                
                spc.AddSource($"{typeDeclaration.Identifier.Text}.systemsgrouprunner_{typeDeclaration.GetStableFileCompliantHash()}.g.cs", sb.ToString());
                
                StringBuilderPool.Return(sb);
                IndentSourcePool.Return(indent);
            });
        }
    }
}