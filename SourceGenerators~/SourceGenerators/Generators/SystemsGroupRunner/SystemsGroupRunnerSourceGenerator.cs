namespace SourceGenerators.Generators.SystemsGroupRunner {
    using System;
    using Microsoft.CodeAnalysis;
    using MorpehHelpers.NonSemantic;
    using Utils.Logging;
    using Utils.NonSemantic;
    using Utils.Pools;
    
    public static class SystemsGroupRunnerSourceGenerator {
        public static void Generate(SourceProductionContext spc, in RunnerToGenerate runner) {
            try {
                var source = Generate(runner);
                spc.AddSource($"{runner.TypeName}.runner_{Guid.NewGuid():N}.g.cs", source);
                
                Logger.Log(nameof(SystemsGroupRunnerSourceGenerator), nameof(Generate), $"Generated systems group: {runner.TypeName}");
            } catch (Exception e) {
                Logger.LogException(nameof(SystemsGroupRunnerSourceGenerator), nameof(Generate), e);
            }
        }
        
        public static string Generate(in RunnerToGenerate runner) {
            var typeName      = runner.TypeName;
            var fields = runner.Fields;

            var sb     = StringBuilderPool.Get();
            var indent = IndentSourcePool.Get();

            sb.AppendMorpehDebugDefines();

            if (runner.TypeNamespace != null) {
                sb.AppendIndent(indent).Append("namespace ").Append(runner.TypeNamespace).AppendLine(" {");
                indent.Right();
            }

            sb.AppendIl2CppAttributes(indent);
            sb.AppendIndent(indent)
                .Append(Types.GetVisibilityModifierString(runner.Visibility))
                .Append(" partial ")
                .Append(Types.AsString(runner.TypeKind))
                .Append(' ')
                .Append(typeName)
                .Append(runner.GenericParams)
                .Append(" : ")
                .Append(KnownTypes.DISPOSABLE_FULL_NAME)
                .Append(' ')
                .Append(runner.GenericConstraints)
                .AppendLine(" {");

            using (indent.Scope()) {
                sb.AppendIndent(indent).AppendLine("private readonly Scellecs.Morpeh.World _world;");
                sb.AppendIndent(indent).AppendLine("private readonly Scellecs.Morpeh.InjectionTable _injectionTable;");

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).Append("public ").Append(typeName).AppendLine("(Scellecs.Morpeh.World world, Scellecs.Morpeh.InjectionTable injectionTable = null) {");
                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, typeName, "Constructor", indent)) {
                        sb.AppendIndent(indent).AppendLine("_world = world;");
                        sb.AppendIndent(indent).AppendLine("_injectionTable = injectionTable;");

                        for (int i = 0, length = fields.Length; i < length; i++) {
                            sb.AppendIndent(indent).Append(fields[i].Name).Append(" = ").Append("new ").Append(fields[i].TypeName).AppendLine("(world, injectionTable);");
                        }

                        sb.AppendIndent(indent).AppendLine("if (injectionTable != null) {");
                        using (indent.Scope()) {
                            for (int i = 0, length = fields.Length; i < length; i++) {
                                sb.AppendIndent(indent).Append(fields[i].Name).AppendLine(".Inject(injectionTable);");
                            }
                        }

                        sb.AppendIndent(indent).AppendLine("}");
                    }
                }
                sb.AppendIndent(indent).AppendLine("}");

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).AppendLine("public void OnAwake() {");
                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, typeName, "OnAwake", indent)) {
                        sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.WorldExtensions.Commit(_world);");

                        for (int i = 0, length = fields.Length; i < length; i++) {
                            sb.AppendIndent(indent).Append(fields[i].Name).AppendLine(".CallAwake();");
                        }

                        sb.AppendIfDefine(MorpehDefines.MORPEH_PROFILING);
                        sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.EndSample();");
                        sb.AppendEndIfDefine();
                    }
                }
                sb.AppendIndent(indent).AppendLine("}");
                
                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).AppendLine("public void CallUpdate(float deltaTime) {");

                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, typeName, "CallUpdate", indent)) {
                        sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.WorldExtensions.Commit(_world);");

                        for (int i = 0, length = fields.Length; i < length; i++) {
                            sb.AppendIndent(indent).Append(fields[i].Name).AppendLine(".CallUpdate(deltaTime);");
                        }
                    }
                }
                sb.AppendIndent(indent).AppendLine("}");

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).AppendLine("public void Dispose() {");
                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, typeName, "Dispose", indent)) {
                        sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.WorldExtensions.Commit(_world);");

                        for (int i = 0, length = fields.Length; i < length; i++) {
                            sb.AppendIndent(indent).Append(fields[i].Name).AppendLine(".CallDispose(_injectionTable);");
                        }
                    }
                }
                sb.AppendIndent(indent).AppendLine("}");
            }
            sb.AppendIndent(indent).AppendLine("}");
            
            if (runner.TypeNamespace != null) {
                indent.Left();
                sb.AppendIndent(indent).AppendLine("}");
            }

            IndentSourcePool.Return(indent);

            return sb.ToStringAndReturn();
        }
    }
}