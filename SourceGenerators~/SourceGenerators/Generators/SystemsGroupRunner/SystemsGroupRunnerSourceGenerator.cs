﻿namespace SourceGenerators.Generators.SystemsGroupRunner {
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using Utils.NonSemantic;
    using Utils.Semantic;
    using Utils.Pools;

    [Generator]
    public class SystemsGroupRunnerSourceGenerator : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.SYSTEMS_GROUP_RUNNER_FULL_NAME,
                (s, _) => s is ClassDeclarationSyntax,
                (ctx, _) => (ctx.TargetNode as ClassDeclarationSyntax, ctx.TargetSymbol as INamedTypeSymbol));
            
            context.RegisterSourceOutput(classes, static (spc, pair) => {
                var (typeDeclaration, typeSymbol) = pair;
                if (typeDeclaration is null || typeSymbol is null) {
                    return;
                }

                var typeName      = typeDeclaration.Identifier.ToString();
                var existingLoops = new HashSet<MorpehLoopTypeSemantic.LoopDefinition>();

                var fields = RunnerFieldDefinitionCache.GetList();
                
                var typeMembers = typeSymbol.GetMembers();
                for (int i = 0, length = typeMembers.Length; i < length; i++) {
                    if (typeMembers[i] is not IFieldSymbol fieldSymbol) {
                        continue;
                    }

                    if (fieldSymbol.Type is not INamedTypeSymbol fieldTypeSymbol) {
                        continue;
                    }

                    var loops = new HashSet<MorpehLoopTypeSemantic.LoopDefinition>();
                    var fieldTypeMembers = fieldTypeSymbol.GetMembers();
                    
                    for (int j = 0, jlength = fieldTypeMembers.Length; j < jlength; j++) {
                        if (fieldTypeMembers[j] is not IFieldSymbol fieldTypeMemberSymbol) {
                            continue;
                        }

                        var loopDefinition = MorpehLoopTypeSemantic.FindLoopType(fieldTypeMemberSymbol.GetAttributes());
                        if (loopDefinition == null) {
                            continue;
                        }

                        loops.Add(loopDefinition.Value);
                    }

                    fields.Add(new RunnerFieldDefinition(
                        typeName: fieldTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                        fieldName: fieldSymbol.Name,
                        loops: loops));
                    
                    existingLoops.UnionWith(loops);
                }

                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                sb.AppendMorpehDebugDefines();
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
                    .AppendGenericConstraints(typeSymbol)
                    .AppendLine(" {");

                using (indent.Scope()) {
                    sb.AppendIndent(indent).AppendLine("private readonly Scellecs.Morpeh.World _world;");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).Append("public ").Append(typeName).AppendLine("(Scellecs.Morpeh.World world, Scellecs.Morpeh.InjectionTable injectionTable = null) {");
                    using (indent.Scope()) {
                        sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.BeginSample(\"").Append(typeName).AppendLine("_Constructor\");");
                        sb.AppendEndIfDefine();
                        
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
                        
                        sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.EndSample();");
                        sb.AppendEndIfDefine();
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void OnAwake() {");
                    using (indent.Scope()) {
                        sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.BeginSample(\"").Append(typeName).AppendLine("_OnAwake\");");
                        sb.AppendEndIfDefine();
                        
                        sb.AppendIndent(indent).AppendLine("_world.Commit();");
                        
                        for (int i = 0, length = fields.Count; i < length; i++) {
                            sb.AppendIndent(indent).Append(fields[i].fieldName).AppendLine(".CallAwake();");
                        }
                        
                        sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.EndSample();");
                        sb.AppendEndIfDefine();
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("public void Dispose() {");
                    using (indent.Scope()) {
                        sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.BeginSample(\"").Append(typeName).AppendLine("_Dispose\");");
                        sb.AppendEndIfDefine();
                        
                        sb.AppendIndent(indent).AppendLine("_world.Commit();");
                        
                        for (int i = 0, length = fields.Count; i < length; i++) {
                            sb.AppendIndent(indent).Append(fields[i].fieldName).AppendLine(".CallDispose();");
                        }
                        
                        sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.EndSample();");
                        sb.AppendEndIfDefine();
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    foreach (var existingLoop in existingLoops) {
                        var methodName = existingLoop.methodName;

                        sb.AppendLine().AppendLine();
                        sb.AppendIndent(indent).Append("public void ").Append(methodName).AppendLine("(float deltaTime) {");

                        using (indent.Scope()) {
                            sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
                            sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.BeginSample(\"").Append(typeName).Append('_').Append(methodName).AppendLine("\");");
                            sb.AppendEndIfDefine();
                            
                            sb.AppendIndent(indent).AppendLine("_world.Commit();");
                            
                            for (int j = 0, jlength = fields.Count; j < jlength; j++) {
                                if (!fields[j].loops.Contains(existingLoop)) {
                                    continue;
                                }
                                
                                sb.AppendIndent(indent).Append(fields[j].fieldName).Append('.').Append(methodName).AppendLine("(deltaTime);");
                            }
                            
                            sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
                            sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.EndSample();");
                            sb.AppendEndIfDefine();
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