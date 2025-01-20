namespace SourceGenerators.Generators.Systems {
    using System;
    using Microsoft.CodeAnalysis;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Options;
    using Utils.Logging;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    public static class SystemSourceGenerator {
        public static void Generate(SourceProductionContext spc, in SystemToGenerate system, in PreprocessorOptionsData options) {
            try {
                var source = Generate(system, options);
                spc.AddSource($"{system.TypeName}.system_{Guid.NewGuid():N}.g.cs", source);
                
                Logger.Log(nameof(SystemSourceGenerator), nameof(Generate), $"Generated system: {system.TypeName}");
            } catch (Exception e) {
                Logger.LogException(nameof(SystemSourceGenerator), nameof(Generate), e);
            }
        }
        
        public static string Generate(in SystemToGenerate system, in PreprocessorOptionsData options) {
            var sb     = StringBuilderPool.Get();
            var indent = IndentSourcePool.Get();

            sb.AppendMorpehDebugDefines();

            if (system.TypeNamespace != null) {
                sb.AppendIndent(indent).Append("namespace ").Append(system.TypeNamespace).AppendLine(" {");
                indent.Right();
            }
            
            var hierarchyDepth = ParentType.WriteOpen(sb, indent, system.Hierarchy);

            sb.AppendIl2CppAttributes(indent);
            sb.AppendIndent(indent)
                .Append(Types.AsString(system.Visibility))
                .Append(" partial ")
                .Append(Types.AsString(system.TypeKind))
                .Append(' ')
                .Append(system.TypeName)
                .Append(system.GenericParams)
                .Append(system.AlwaysEnabled ? " : Scellecs.Morpeh.ISystem " : " : Scellecs.Morpeh.IEnableableSystem ")
                .Append(system.GenericConstraints)
                .AppendLine(" {");

            using (indent.Scope()) {
                sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                sb.AppendIndent(indent).AppendLine("private bool _systemHasFailed;");
                sb.AppendEndIfDefine();

                if (system.StashRequirements.Length > 0) {
                    sb.AppendLine().AppendLine();
                    for (int i = 0, length = system.StashRequirements.Length; i < length; i++) {
                        var stash = system.StashRequirements[i];
                        var stashVariation = options.EnableStashSpecialization ? stash.StashVariation : StashVariation.Data;
                        sb.AppendIndent(indent).Append("private readonly ").Append(MorpehComponentHelpersSemantic.GetStashSpecializationType(stashVariation, stash.MetadataClassName)).Append(' ').Append(stash.FieldName).AppendLine(";");
                    }
                }

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).Append("public ").Append(system.TypeName).AppendLine("(Scellecs.Morpeh.World world) {");
                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, system.TypeName, "Constructor", indent, isUnityProfiler: options.IsUnityProfiler)) {
                        sb.AppendIndent(indent).AppendLine("World = world;");

                        for (int i = 0, length = system.StashRequirements.Length; i < length; i++) {
                            var stash = system.StashRequirements[i];
                            sb.AppendIndent(indent).Append(stash.FieldName).Append(" = ").Append(stash.MetadataClassName).AppendLine(".GetStash(world);");
                        }
                    }
                }

                sb.AppendIndent(indent).AppendLine("}");

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).AppendLine("public void CallAwake() {");
                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, system.TypeName, "Awake", indent, isUnityProfiler: options.IsUnityProfiler)) {
                        sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                        sb.AppendIndent(indent).AppendLine("try {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("OnAwake();");
                        }

                        sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(system.TypeName).AppendLine(" system (OnAwake), the system will be disabled\");");
                            sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
                            sb.AppendIndent(indent).AppendLine("_systemHasFailed = true;");
                        }

                        sb.AppendIndent(indent).AppendLine("}");
                        sb.AppendElseDefine();
                        sb.AppendIndent(indent).AppendLine("OnAwake();");
                        sb.AppendEndIfDefine();

                        sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.WorldExtensions.Commit(World);");
                    }
                }

                sb.AppendIndent(indent).AppendLine("}");

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).AppendLine("public void CallUpdate(float deltaTime) {");
                using (indent.Scope()) {
                    sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                    sb.AppendIndent(indent).AppendLine("if (_systemHasFailed) {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).AppendLine("return;");
                    }

                    sb.AppendIndent(indent).AppendLine("}");
                    sb.AppendEndIfDefine();

                    if (!system.AlwaysEnabled) {
                        sb.AppendIndent(indent).AppendLine("if (!IsEnabled()) {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("return;");
                        }

                        sb.AppendIndent(indent).AppendLine("}");
                    }

                    using (MorpehSyntax.ScopedProfile(sb, system.TypeName, "OnUpdate", indent, isUnityProfiler: options.IsUnityProfiler)) {
                        sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                        sb.AppendIndent(indent).AppendLine("try {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("OnUpdate(deltaTime);");
                        }

                        sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(system.TypeName)
                                .AppendLine(" system (OnUpdate), the system will be disabled\");");
                            sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
                            sb.AppendIndent(indent).AppendLine("_systemHasFailed = true;");
                        }

                        sb.AppendIndent(indent).AppendLine("}");
                        sb.AppendElseDefine();
                        sb.AppendIndent(indent).AppendLine("OnUpdate(deltaTime);");
                        sb.AppendEndIfDefine();

                        if (!system.SkipCommit) {
                            sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.WorldExtensions.Commit(World);");
                        }
                    }
                }

                sb.AppendIndent(indent).AppendLine("}");

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).AppendLine("public void CallDispose() {");
                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, system.TypeName, "Dispose", indent, isUnityProfiler: options.IsUnityProfiler)) {
                        sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                        sb.AppendIndent(indent).AppendLine("try {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("Dispose();");
                        }

                        sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(system.TypeName).AppendLine(" system (Dispose), the system will be disabled\");");
                            sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
                            sb.AppendIndent(indent).AppendLine("_systemHasFailed = true;");
                        }

                        sb.AppendIndent(indent).AppendLine("}");
                        sb.AppendElseDefine();
                        sb.AppendIndent(indent).AppendLine("Dispose();");
                        sb.AppendEndIfDefine();

                        sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.WorldExtensions.Commit(World);");
                    }
                }

                sb.AppendIndent(indent).AppendLine("}");
            }

            sb.AppendIndent(indent).AppendLine("}");
            
            ParentType.WriteClose(sb, indent, hierarchyDepth);

            if (system.TypeNamespace != null) {
                indent.Left();
                sb.AppendIndent(indent).AppendLine("}");
            }

            IndentSourcePool.Return(indent);
            return sb.ToStringAndReturn();
        }
    }
}