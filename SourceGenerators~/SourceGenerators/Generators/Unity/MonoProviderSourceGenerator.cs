namespace SourceGenerators.Generators.Unity {
    using System;
    using Microsoft.CodeAnalysis;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Options;
    using Utils.Logging;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    public static class MonoProviderSourceGenerator {
        public static void Generate(SourceProductionContext spc, in ProviderToGenerate provider, in PreprocessorOptionsData options) {
            try {
                var source = Generate(provider, options);
                spc.AddSource($"{provider.TypeName}._provider_{Guid.NewGuid():N}.g.cs", source);
                
                Logger.Log(nameof(MonoProviderSourceGenerator), nameof(Generate), $"Generated: {provider.TypeName}");
            } catch (Exception e) {
                Logger.LogException(nameof(MonoProviderSourceGenerator), nameof(Generate), e);
            }
        }
        
        public static string Generate(ProviderToGenerate provider, PreprocessorOptionsData options) {
            var stashVariation     = options.EnableStashSpecialization ? provider.StashVariation : StashVariation.Data;
            var specializationType = MorpehComponentHelpersSemantic.GetStashSpecializationType(stashVariation, provider.ProviderTypeFullName);
            var typeName           = provider.TypeName;

            var sb     = StringBuilderPool.Get();
            var indent = IndentSourcePool.Get();

            sb.AppendIndent(indent).AppendLine("using Sirenix.OdinInspector;");
            sb.AppendIndent(indent).AppendLine("using UnityEngine;");
            sb.AppendIndent(indent).AppendLine("using Scellecs.Morpeh;");

            if (provider.TypeNamespace != null) {
                sb.AppendIndent(indent).Append("namespace ").Append(provider.TypeNamespace).AppendLine(" {");
                indent.Right();
            }
            
            var hierarchyDepth = ParentType.WriteOpen(sb, indent, provider.Hierarchy);

            sb.AppendIl2CppAttributes(indent);
            sb.AppendIndent(indent)
                .Append(Types.AsString(provider.Visibility))
                .Append(" partial class ")
                .Append(typeName)
                .Append(provider.GenericParams)
                .Append(" : Scellecs.Morpeh.Providers.EntityProvider ")
                .Append(provider.GenericConstraints)
                .AppendLine(" {");

            using (indent.Scope()) {
                if (stashVariation != StashVariation.Tag) {
                    sb.AppendIndent(indent).AppendLine("[SerializeField]");
                    sb.AppendIndent(indent).AppendLine("[HideInInspector]");
                    sb.AppendIndent(indent).Append("private ").Append(provider.ProviderTypeFullName).AppendLine(" serializedData;");
                }

                sb.AppendIndent(indent).Append("private ").Append(specializationType).AppendLine(" stash;");

                if (stashVariation != StashVariation.Tag) {
                    sb.AppendLine().AppendLine();
                    sb.AppendIfDefine("UNITY_EDITOR");
                    sb.AppendIndent(indent).AppendLine("[PropertySpace]");
                    sb.AppendIndent(indent).AppendLine("[ShowInInspector]");
                    sb.AppendIndent(indent).AppendLine("[PropertyOrder(1)]");
                    sb.AppendIndent(indent).AppendLine("[HideLabel]");
                    sb.AppendIndent(indent).AppendLine("[InlineProperty]");
                    sb.AppendEndIfDefine();
                    sb.AppendIndent(indent).Append("private ").Append(provider.ProviderTypeFullName).AppendLine(" Data {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).AppendLine("get {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("if (World.Default?.Has(this.cachedEntity) == true) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("var data = this.Stash.Get(this.cachedEntity, out var exist);").AppendLine();
                                sb.AppendIndent(indent).Append("if (exist) {").AppendLine();
                                using (indent.Scope()) {
                                    sb.AppendIndent(indent).Append("return data;").AppendLine();
                                }

                                sb.AppendIndent(indent).AppendLine("}");
                            }

                            sb.AppendIndent(indent).AppendLine("}");
                            sb.AppendIndent(indent).AppendLine("return this.serializedData;");
                        }

                        sb.AppendIndent(indent).AppendLine("}");

                        sb.AppendIndent(indent).AppendLine("set {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("if (World.Default?.Has(this.cachedEntity) == true) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("this.Stash.Set(this.cachedEntity, value);").AppendLine();
                            }

                            sb.AppendIndent(indent).AppendLine("}");
                            sb.AppendIndent(indent).AppendLine("else {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("this.serializedData = value;").AppendLine();
                            }

                            sb.AppendIndent(indent).AppendLine("}");
                        }

                        sb.AppendIndent(indent).AppendLine("}");
                    }

                    sb.AppendIndent(indent).AppendLine("}");
                }

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).Append(Types.AsString(provider.ProviderTypeVisibility)).Append(" ").Append(specializationType).AppendLine(" Stash {");
                using (indent.Scope()) {
                    sb.AppendIndent(indent).AppendLine("get {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).AppendLine("if (this.stash == null) {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).Append("this.stash = ").Append(provider.ProviderTypeFullName).AppendLine(".GetStash(World.Default);");
                        }

                        sb.AppendIndent(indent).AppendLine("}");
                        sb.AppendIndent(indent).AppendLine("return this.stash;");
                    }

                    sb.AppendIndent(indent).AppendLine("}");
                }

                sb.AppendIndent(indent).AppendLine("}");

                if (stashVariation != StashVariation.Tag) {
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).Append("public ref ").Append(provider.ProviderTypeFullName).AppendLine(" GetSerializedData() => ref this.serializedData;");

                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).Append("public ref ").Append(provider.ProviderTypeFullName).AppendLine(" GetData() {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).Append("var ent = this.Entity;").AppendLine();
                        sb.AppendIndent(indent).Append("if (World.Default?.Has(this.cachedEntity) == true) {").AppendLine();
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).Append("if (this.Stash.Has(ent)) {").AppendLine();
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("return ref this.Stash.Get(ent);").AppendLine();
                            }

                            sb.AppendIndent(indent).AppendLine("}");
                        }

                        sb.AppendIndent(indent).AppendLine("}");
                        sb.AppendIndent(indent).AppendLine("return ref this.serializedData;");
                    }

                    sb.AppendIndent(indent).AppendLine("}");

                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).Append("public ref ").Append(provider.ProviderTypeFullName).AppendLine(" GetData(out bool existOnEntity) {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).Append("if (World.Default?.Has(this.cachedEntity) == true) {").AppendLine();
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).Append("return ref this.Stash.Get(this.cachedEntity, out existOnEntity);").AppendLine();
                        }

                        sb.AppendIndent(indent).AppendLine("}");
                        sb.AppendIndent(indent).AppendLine("existOnEntity = false;");
                        sb.AppendIndent(indent).AppendLine("return ref this.serializedData;");
                    }

                    sb.AppendIndent(indent).AppendLine("}");
                }

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).AppendLine("protected sealed override void PreInitialize() {");
                using (indent.Scope()) {
                    sb.AppendIndent(indent).AppendLine(stashVariation == StashVariation.Tag
                        ? "this.Stash.Set(this.cachedEntity);"
                        : "this.Stash.Set(this.cachedEntity, this.serializedData);");
                }

                sb.AppendIndent(indent).AppendLine("}");

                sb.AppendLine().AppendLine();
                sb.AppendIndent(indent).AppendLine("protected sealed override void PreDeinitialize() {");
                using (indent.Scope()) {
                    sb.AppendIndent(indent).AppendLine("var ent = this.Entity;");
                    sb.AppendIndent(indent).AppendLine("if (World.Default?.Has(ent) == true) {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).Append("this.Stash.Remove(ent);").AppendLine();
                    }

                    sb.AppendIndent(indent).AppendLine("}");
                }

                sb.AppendIndent(indent).AppendLine("}");
            }

            sb.AppendIndent(indent).AppendLine("}");
            
            ParentType.WriteClose(sb, indent, hierarchyDepth);

            if (provider.TypeNamespace != null) {
                indent.Left();
                sb.AppendIndent(indent).AppendLine("}");
            }
            
            IndentSourcePool.Return(indent);

            return sb.ToStringAndReturn();
        }
    }
}