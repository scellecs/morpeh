namespace SourceGenerators.Generators.SystemsGroupRunner {
    using System;
    using Microsoft.CodeAnalysis;
    using MorpehHelpers.NonSemantic;
    using Options;
    using Utils.Logging;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    public static class SystemsGroupRunnerSourceGenerator {
        public static void Generate(SourceProductionContext spc, in RunnerToGenerate runner, in PreprocessorOptionsData options) {
            try {
                var source = Generate(runner, options);
                spc.AddSource($"{runner.TypeName}.runner_{Guid.NewGuid():N}.g.cs", source);
                
                Logger.Log(nameof(SystemsGroupRunnerSourceGenerator), nameof(Generate), $"Generated systems group: {runner.TypeName}");
            } catch (Exception e) {
                Logger.LogException(nameof(SystemsGroupRunnerSourceGenerator), nameof(Generate), e);
            }
        }
        
        public static string Generate(in RunnerToGenerate runner, in PreprocessorOptionsData options) {
            var fields = runner.Fields;

            var sb     = StringBuilderPool.Get();
            var indent = IndentSourcePool.Get();

            sb.AppendMorpehDebugDefines();

            if (runner.TypeNamespace != null) {
                sb.AppendIndent(indent).Append("namespace ").Append(runner.TypeNamespace).AppendLine(" {");
                indent.Right();
            }
            
            var hierarchyDepth = ParentType.WriteOpen(sb, indent, runner.Hierarchy);

            sb.AppendIl2CppAttributes(indent);
            sb.AppendIndent(indent)
                .Append(Types.AsString(runner.Visibility))
                .Append(" partial ")
                .Append(Types.AsString(runner.TypeKind))
                .Append(' ')
                .Append(runner.TypeName)
                .Append(" : ")
                .Append(KnownTypes.DISPOSABLE_FULL_NAME)
                .AppendLine(" {");

            using (indent.Scope()) {
                sb.AppendIndent(indent).AppendLine("private readonly Scellecs.Morpeh.World _world;");
                sb.AppendIndent(indent).AppendLine("private readonly Scellecs.Morpeh.InjectionTable _injectionTable;");

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).Append("public ").Append(runner.TypeName).AppendLine("(Scellecs.Morpeh.World world, Scellecs.Morpeh.InjectionTable injectionTable = null) {");
                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, runner.TypeName, "Constructor", indent, isUnityProfiler: options.IsUnityProfiler)) {
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
                    using (MorpehSyntax.ScopedProfile(sb, runner.TypeName, "OnAwake", indent, isUnityProfiler: options.IsUnityProfiler)) {
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
                    using (MorpehSyntax.ScopedProfile(sb, runner.TypeName, "CallUpdate", indent, isUnityProfiler: options.IsUnityProfiler)) {
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
                    using (MorpehSyntax.ScopedProfile(sb, runner.TypeName, "Dispose", indent, isUnityProfiler: options.IsUnityProfiler)) {
                        sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.WorldExtensions.Commit(_world);");

                        for (int i = 0, length = fields.Length; i < length; i++) {
                            sb.AppendIndent(indent).Append(fields[i].Name).AppendLine(".CallDispose(_injectionTable);");
                        }
                    }
                }
                sb.AppendIndent(indent).AppendLine("}");
            }
            sb.AppendIndent(indent).AppendLine("}");
            
            ParentType.WriteClose(sb, indent, hierarchyDepth);
            
            if (runner.TypeNamespace != null) {
                indent.Left();
                sb.AppendIndent(indent).AppendLine("}");
            }

            IndentSourcePool.Return(indent);

            return sb.ToStringAndReturn();
        }
    }
}