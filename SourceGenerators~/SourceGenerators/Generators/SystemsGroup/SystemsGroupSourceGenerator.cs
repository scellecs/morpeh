namespace SourceGenerators.Generators.SystemsGroup {
    using System;
    using System.Runtime.CompilerServices;
    using Microsoft.CodeAnalysis;
    using MorpehHelpers.NonSemantic;
    using Options;
    using Utils.Logging;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    public static class SystemsGroupSourceGenerator {
        public static void Generate(SourceProductionContext spc, in SystemsGroupToGenerate systemsGroup, in PreprocessorOptionsData options) {
            try {
                var source = Generate(systemsGroup, options);
                spc.AddSource($"{systemsGroup.TypeName}.systemsgroup_{Guid.NewGuid():N}.g.cs", source);
                
                Logger.Log(nameof(SystemsGroupSourceGenerator), nameof(Generate), $"Generated systems group: {systemsGroup.TypeName}");
            } catch (Exception e) {
                Logger.LogException(nameof(SystemsGroupSourceGenerator), nameof(Generate), e);
            }
        }
        
        public static string Generate(in SystemsGroupToGenerate systemsGroup, in PreprocessorOptionsData options) {
            var fields = systemsGroup.Fields;
            
            var sb     = StringBuilderPool.Get();
            var indent = IndentSourcePool.Get();

            sb.AppendMorpehDebugDefines();

            if (systemsGroup.TypeNamespace != null) {
                sb.AppendIndent(indent).Append("namespace ").Append(systemsGroup.TypeNamespace).AppendLine(" {");
                indent.Right();
            }

            var hierarchyDepth = ParentType.WriteOpen(sb, indent, systemsGroup.Hierarchy);

            sb.AppendIl2CppAttributes(indent);
            sb.AppendIndent(indent)
                .Append(Types.AsString(systemsGroup.Visibility))
                .Append(" partial ")
                .Append(Types.AsString(systemsGroup.TypeKind))
                .Append(' ')
                .Append(systemsGroup.TypeName)
                .AppendLine(" {");

            using (indent.Scope()) {
                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).Append("public ").Append(systemsGroup.TypeName).AppendLine("(Scellecs.Morpeh.World world, Scellecs.Morpeh.InjectionTable injectionTable = null) {");
                using (indent.Scope()) {
                    for (int i = 0, length = fields.Length; i < length; i++) {
                        var field = fields[i];

                        if (field.FieldKind is SystemsGroupFieldKind.System or SystemsGroupFieldKind.Initializer) {
                            sb.AppendIndent(indent).Append(field.Name).Append(" = new ").Append(field.TypeName).AppendLine("(world);");
                        } else {
                            sb.AppendIndent(indent).Append(field.Name).Append(" = new ").Append(field.TypeName).AppendLine("();");
                        }
                    }

                    if (systemsGroup.HasRegistrations) {
                        sb.AppendIndent(indent).AppendLine("if (injectionTable != null) {");
                        using (indent.Scope()) {
                            for (int i = 0, length = fields.Length; i < length; i++) {
                                var field = fields[i];

                                if (field.RegisterAs == null) {
                                    continue;
                                }

                                if (field.RegisterAs == field.TypeName) {
                                    sb.AppendIndent(indent).Append("injectionTable.Register(").Append(field.Name).AppendLine(");");
                                } else {
                                    sb.AppendIndent(indent).Append("injectionTable.Register(").Append(field.Name).Append(", typeof(").Append(field.RegisterAs).AppendLine("));");
                                }
                            }
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                    }
                }
                sb.AppendIndent(indent).AppendLine("}");

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).AppendLine("public void Inject(Scellecs.Morpeh.InjectionTable injectionTable) {");
                using (indent.Scope()) {
                    for (int i = 0, length = fields.Length; i < length; i++) {
                        var field = fields[i];
                        
                        if (field.IsInjectable) {
                            sb.AppendIndent(indent).Append(field.Name).AppendLine(".Inject(injectionTable);");
                        }
                    }
                }
                sb.AppendIndent(indent).AppendLine("}");

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).AppendLine("public void CallAwake() {");
                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, systemsGroup.TypeName, "CallAwake", indent, isUnityProfiler: options.IsUnityProfiler)) {
                        for (int i = 0, length = fields.Length; i < length; i++) {
                            var field = fields[i];
                            
                            if (field.FieldKind is SystemsGroupFieldKind.System or SystemsGroupFieldKind.Initializer) {
                                sb.AppendIndent(indent).Append(field.Name).AppendLine(".CallAwake();");
                            }
                        }
                    }
                }
                sb.AppendIndent(indent).AppendLine("}");
                
                sb.AppendLine().AppendLine();
                if (systemsGroup.InlineUpdateMethods) {
                    sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                }
                sb.AppendIndent(indent).AppendLine("public void CallUpdate(float deltaTime) {");
                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, systemsGroup.TypeName, "CallUpdate", indent, isUnityProfiler: options.IsUnityProfiler)) {
                        for (int i = 0, length = fields.Length; i < length; i++) {
                            var field = fields[i];
                            
                            if (field.FieldKind is SystemsGroupFieldKind.System) {
                                sb.AppendIndent(indent).Append(field.Name).AppendLine(".CallUpdate(deltaTime);");
                            }
                        }
                    }
                }
                sb.AppendIndent(indent).AppendLine("}");

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).AppendLine("public void CallDispose(Scellecs.Morpeh.InjectionTable injectionTable = null) {");
                using (indent.Scope()) {
                    using (MorpehSyntax.ScopedProfile(sb, systemsGroup.TypeName, "CallDispose", indent, isUnityProfiler: options.IsUnityProfiler)) {
                        for (int i = 0, length = fields.Length; i < length; i++) {
                            var field = fields[i];
                            
                            if (field.FieldKind is SystemsGroupFieldKind.System or SystemsGroupFieldKind.Initializer) {
                                sb.AppendIndent(indent).AppendLine($"{field.Name}.CallDispose();");
                            } else if (field.FieldKind is SystemsGroupFieldKind.Disposable) {
                                sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                                sb.AppendIndent(indent).AppendLine("try {");
                                sb.AppendEndIfDefine();
                                using (indent.Scope()) {
                                    sb.AppendIndent(indent).AppendLine($"{field.Name}.Dispose();");
                                }

                                sb.AppendIfDefine(MorpehDefines.MORPEH_DEBUG);
                                sb.AppendIndent(indent).AppendLine("} catch (global::System.Exception exception) {");
                                using (indent.Scope()) {
                                    sb.AppendIndent(indent).Append("Scellecs.Morpeh.MLogger.LogError(\"Exception in ").Append(systemsGroup.TypeName).AppendLine(" (Dispose)\");");
                                    sb.AppendIndent(indent).AppendLine("Scellecs.Morpeh.MLogger.LogException(exception);");
                                }

                                sb.AppendIndent(indent).AppendLine("}");
                                sb.AppendEndIfDefine();
                            }
                        }

                        if (systemsGroup.HasRegistrations) {
                            sb.AppendIndent(indent).AppendLine("if (injectionTable != null) {");
                            using (indent.Scope()) {
                                for (int i = 0, length = fields.Length; i < length; i++) {
                                    var field = fields[i];

                                    if (field.RegisterAs == null) {
                                        continue;
                                    }

                                    if (field.RegisterAs == field.TypeName) {
                                        sb.AppendIndent(indent).Append("injectionTable.UnRegister(").Append(field.Name).AppendLine(");");
                                    } else {
                                        sb.AppendIndent(indent).Append("injectionTable.UnRegister(typeof(").Append(field.RegisterAs).AppendLine("));");
                                    }
                                }
                            }
                            sb.AppendIndent(indent).AppendLine("}");
                        }
                    }
                }
                sb.AppendIndent(indent).AppendLine("}");
            }
            sb.AppendIndent(indent).AppendLine("}");
            
            ParentType.WriteClose(sb, indent, hierarchyDepth);
            
            if (systemsGroup.TypeNamespace != null) {
                indent.Left();
                sb.AppendIndent(indent).AppendLine("}");
            }
            
            IndentSourcePool.Return(indent);

            return sb.ToStringAndReturn();
        }
    }
}