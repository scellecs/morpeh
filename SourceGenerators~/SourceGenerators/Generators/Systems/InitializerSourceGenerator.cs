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

    public static class InitializerSourceGenerator {
        public static void Generate(SourceProductionContext spc, in InitializerToGenerate initializer, in PreprocessorOptionsData options) {
            try {
                var source = Generate(initializer, options);
                spc.AddSource($"{initializer.TypeName}.initializer_{Guid.NewGuid():N}.g.cs", source);

                Logger.Log(nameof(InitializerSourceGenerator), nameof(Generate), $"Generated initializer: {initializer.TypeName}");
            } catch (Exception e) {
                Logger.LogException(nameof(InitializerSourceGenerator), nameof(Generate), e);
            }
        }

        public static string Generate(in InitializerToGenerate initializer, in PreprocessorOptionsData options) {
            var sb     = StringBuilderPool.Get();
            var indent = IndentSourcePool.Get();

            sb.AppendMorpehDebugDefines();

            if (initializer.TypeNamespace != null) {
                sb.AppendIndent(indent).Append("namespace ").Append(initializer.TypeNamespace).AppendLine(" {");
                indent.Right();
            }
            
            var hierarchyDepth = ParentType.WriteOpen(sb, indent, initializer.Hierarchy);

            sb.AppendIl2CppAttributes(indent);
            sb.AppendIndent(indent)
                .Append(Types.AsString(initializer.Visibility))
                .Append(" partial ")
                .Append(Types.AsString(initializer.TypeKind))
                .Append(' ')
                .Append(initializer.TypeName)
                .Append(initializer.GenericParams)
                .Append(" : Scellecs.Morpeh.IInitializer ")
                .Append(initializer.GenericConstraints)
                .AppendLine(" {");


            using (indent.Scope()) {
                if (initializer.StashRequirements.Length > 0) {
                    sb.AppendLine().AppendLine();
                    for (int i = 0, length = initializer.StashRequirements.Length; i < length; i++) {
                        var stash = initializer.StashRequirements[i];
                        var stashVariation = options.EnableStashSpecialization ? stash.StashVariation : StashVariation.Data;
                        sb.AppendIndent(indent).Append("private readonly ").Append(MorpehComponentHelpersSemantic.GetStashSpecializationType(stashVariation, stash.MetadataClassName)).Append(' ').Append(stash.FieldName).AppendLine(";");
                    }
                }

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).Append("public ").Append(initializer.TypeName).AppendLine("(Scellecs.Morpeh.World world) {");
                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, initializer.TypeName, "Constructor", indent, isUnityProfiler: options.IsUnityProfiler)) {
                        sb.AppendIndent(indent).AppendLine("World = world;");

                        for (int i = 0, length = initializer.StashRequirements.Length; i < length; i++) {
                            var stash = initializer.StashRequirements[i];
                            sb.AppendIndent(indent).Append(stash.FieldName).Append(" = ").Append(stash.MetadataClassName).AppendLine(".GetStash(world);");
                        }
                    }
                }

                sb.AppendIndent(indent).AppendLine("}");

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).AppendLine("public void CallAwake() {");
                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, initializer.TypeName, "Awake", indent, isUnityProfiler: options.IsUnityProfiler)) {
                        sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                        sb.AppendIndent(indent).AppendLine("try {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("OnAwake();");
                        }

                        sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(initializer.TypeName).AppendLine(" initializer (OnAwake)\");");
                            sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
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
                sb.AppendIndent(indent).AppendLine("public void CallDispose() {");
                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, initializer.TypeName, "Dispose", indent, isUnityProfiler: options.IsUnityProfiler)) {
                        sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                        sb.AppendIndent(indent).AppendLine("try {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("Dispose();");
                        }

                        sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(initializer.TypeName).AppendLine(" initializer (Dispose)\");");
                            sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
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
            
            if (initializer.TypeNamespace != null) {
                indent.Left();
                sb.AppendIndent(indent).AppendLine("}");
            }

            IndentSourcePool.Return(indent);
            return sb.ToStringAndReturn();
        }
    }
}