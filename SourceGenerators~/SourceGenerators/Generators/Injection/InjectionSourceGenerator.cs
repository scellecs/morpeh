﻿namespace SourceGenerators.Generators.Injection {
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Utils.Logging;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    public static class InjectionSourceGenerator {
        public static void Generate(SourceProductionContext spc, in InjectionToGenerate injection, ImmutableArray<GenericResolver> genericResolvers) {
            try {
                var source = Generate(injection, genericResolvers);
                spc.AddSource($"{injection.TypeName}.injection_{Guid.NewGuid():N}.g.cs", source);

                Logger.Log(nameof(InjectionSourceGenerator), nameof(Generate), $"Generated injection: {injection.TypeName}");
            } catch (Exception e) {
                Logger.LogException(nameof(InjectionSourceGenerator), nameof(Generate), e);
            }
        }
        
        public static string Generate(in InjectionToGenerate injection, ImmutableArray<GenericResolver> genericResolvers) {
            var sb     = StringBuilderPool.Get();
            var indent = IndentSourcePool.Get();

            if (injection.TypeNamespace != null) {
                sb.AppendIndent(indent).Append("namespace ").Append(injection.TypeNamespace).AppendLine(" {");
                indent.Right();
            }

            var hierarchyDepth = ParentType.WriteOpen(sb, indent, injection.Hierarchy);

            sb.AppendIndent(indent)
                .Append(Types.AsString(injection.Visibility))
                .Append(" partial ")
                .Append(Types.AsString(injection.TypeKind))
                .Append(' ')
                .Append(injection.TypeName)
                .Append(injection.GenericParams)
                .Append(" : Scellecs.Morpeh.IInjectable ")
                .Append(injection.GenericConstraints)
                .AppendLine(" {");

            using (indent.Scope()) {
                sb.AppendIndent(indent).AppendLine(injection.HasInjectionsInParents
                    ? "public override void Inject(Scellecs.Morpeh.InjectionTable injectionTable) {"
                    : "public virtual void Inject(Scellecs.Morpeh.InjectionTable injectionTable) {");

                using (indent.Scope()) {
                    if (injection.HasInjectionsInParents) {
                        sb.AppendIndent(indent).AppendLine("base.Inject(injectionTable);");
                    }

                    for (int i = 0, length = injection.Fields.Length; i < length; i++) {
                        var field = injection.Fields[i];

                        var resolved = false;

                        if (field.GenericParams != null) {
                            for (int j = 0, genericResolversLength = genericResolvers.Length; j < genericResolversLength; j++) {
                                var genericResolver = genericResolvers[j];
                                if (field.TypeName != genericResolver.BaseTypeName) {
                                    continue;
                                }

                                sb.AppendIndent(indent)
                                    .Append(field.Name)
                                    .Append(" = ((")
                                    .Append(genericResolver.ResolverTypeName)
                                    .Append(")injectionTable.Get(typeof(")
                                    .Append(genericResolver.ResolverTypeName)
                                    .Append("))).Resolve")
                                    .Append(field.GenericParams)
                                    .AppendLine("();");
                                
                                resolved = true;
                                break;
                            }
                        }

                        if (resolved) {
                            continue;
                        }

                        sb.AppendIndent(indent).Append(field.Name).Append(" = (").Append(field.TypeName).Append(")injectionTable.Get(typeof(").Append(field.TypeName).AppendLine("));");
                    }
                }

                sb.AppendIndent(indent).AppendLine("}");
            }

            sb.AppendIndent(indent).AppendLine("}");

            ParentType.WriteClose(sb, indent, hierarchyDepth);

            if (injection.TypeNamespace != null) {
                indent.Left();
                sb.AppendIndent(indent).AppendLine("}");
            }

            IndentSourcePool.Return(indent);

            return sb.ToStringAndReturn();
        }
    }
}