namespace SourceGenerators.Generators.Components {
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Options;
    using Utils.Logging;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    public static class ComponentSourceGenerator {
        public static void Generate(SourceProductionContext spc, in ComponentToGenerate component, in PreprocessorOptionsData options) {
            try {
                var source = Generate(component, options);
                spc.AddSource($"{component.TypeName}.component_{Guid.NewGuid():N}.g.cs", source);
                
                Logger.Log(nameof(ComponentSourceGenerator), nameof(Generate), $"Generated component: {component.TypeName}");
            } catch (Exception e) {
                Logger.LogException(nameof(ComponentSourceGenerator), nameof(Generate), e);
            }
        }
        
        public static string Generate(in ComponentToGenerate component, in PreprocessorOptionsData options) {
            var fullTypeName   = StringBuilderPool.Get().Append(component.TypeName).Append(component.GenericParams).ToStringAndReturn();
            var stashVariation = options.EnableStashSpecialization ? component.StashVariation : StashVariation.Data;

            var sb     = StringBuilderPool.Get();
            var indent = IndentSourcePool.Get();

            if (component.TypeNamespace != null) {
                sb.AppendIndent(indent).Append("namespace ").Append(component.TypeNamespace).AppendLine(" {");
                indent.Right();
            }
            
            var hierarchyDepth = ParentType.WriteOpen(sb, indent, component.Hierarchy);

            sb.AppendIl2CppAttributes(indent);
            sb.AppendIndent(indent)
                .Append(Types.AsString(component.Visibility))
                .Append(" partial struct ")
                .Append(component.TypeName)
                .Append(component.GenericParams)
                .Append(" : ")
                .Append(MorpehComponentHelpersSemantic.GetStashSpecializationConstraintInterface(stashVariation))
                .Append(' ')
                .Append(component.GenericConstraints)
                .AppendLine(" {");

            using (indent.Scope()) {
                sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                sb.AppendIndent(indent)
                    .Append("public static ")
                    .Append(MorpehComponentHelpersSemantic.GetStashSpecializationType(stashVariation, fullTypeName))
                    .Append(" GetStash(Scellecs.Morpeh.World world) => Scellecs.Morpeh.WorldStashExtensions.")
                    .Append(MorpehComponentHelpersSemantic.GetStashSpecializationGetStashMethod(stashVariation, fullTypeName))
                    .Append("(world, capacity: ")
                    .Append(component.InitialCapacity)
                    .AppendLine(");");

                if (options.EnableSlowComponentApi) {
                    // Has
                    sb.AppendLine();
                    sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                    sb.AppendIndent(indent).AppendLine("public static bool Has(Scellecs.Morpeh.Entity entity) {");
                    using (indent.Scope()) {
                        AppendEntityLivelinessCheck();
                        sb.AppendIndent(indent).AppendLine("return GetStash(world).Has(entity);");
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    // Set
                    sb.AppendLine();
                    sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                    sb.AppendIndent(indent).AppendLine("public static void Set(Scellecs.Morpeh.Entity entity) {");
                    using (indent.Scope()) {
                        AppendEntityLivelinessCheck();
                        sb.AppendIndent(indent).AppendLine("GetStash(world).Set(entity);");
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    // Remove
                    sb.AppendLine();
                    sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                    sb.AppendIndent(indent).AppendLine("public static void Remove(Scellecs.Morpeh.Entity entity) {");
                    using (indent.Scope()) {
                        AppendEntityLivelinessCheck();
                        sb.AppendIndent(indent).AppendLine("GetStash(world).Remove(entity);");
                    }
                    sb.AppendIndent(indent).AppendLine("}");

                    if (stashVariation != StashVariation.Tag) {
                        // Set + in
                        sb.AppendLine();
                        sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                        sb.AppendIndent(indent).Append("public static void Set(Scellecs.Morpeh.Entity entity, in ").Append(fullTypeName).AppendLine(" value) {");
                        using (indent.Scope()) {
                            AppendEntityLivelinessCheck();
                            sb.AppendIndent(indent).AppendLine("GetStash(world).Set(entity, value);");
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                        
                        // Add
                        sb.AppendLine();
                        sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                        sb.AppendIndent(indent).Append("public static ref ").Append(fullTypeName).AppendLine(" Add(Scellecs.Morpeh.Entity entity) {");
                        using (indent.Scope()) {
                            AppendEntityLivelinessCheck();
                            sb.AppendIndent(indent).AppendLine("return ref GetStash(world).Add(entity);");
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                        
                        // Add + out
                        sb.AppendLine();
                        sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                        sb.AppendIndent(indent).Append("public static ref ").Append(fullTypeName).AppendLine(" Add(Scellecs.Morpeh.Entity entity, out bool exist) {");
                        using (indent.Scope()) {
                            AppendEntityLivelinessCheck();
                            sb.AppendIndent(indent).AppendLine("var stash = GetStash(world);");
                            sb.AppendIndent(indent).AppendLine("exist = stash.Has(entity);");
                            sb.AppendIndent(indent).AppendLine("if (!exist) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("return ref stash.Add(entity);");
                            }
                            sb.AppendIndent(indent).AppendLine("}");
                            sb.AppendIndent(indent).AppendLine("return ref stash.Empty;");
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                        
                        // Get
                        sb.AppendLine();
                        sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                        sb.AppendIndent(indent).Append("public static ref ").Append(fullTypeName).AppendLine(" Get(Scellecs.Morpeh.Entity entity) {");
                        using (indent.Scope()) {
                            AppendEntityLivelinessCheck();
                            sb.AppendIndent(indent).AppendLine("return ref GetStash(world).Get(entity);");
                        }
                        sb.AppendIndent(indent).AppendLine("}");

                        // Get + out
                        sb.AppendLine();
                        sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                        sb.AppendIndent(indent).Append("public static ref ").Append(fullTypeName).AppendLine(" Get(Scellecs.Morpeh.Entity entity, out bool success) {");
                        using (indent.Scope()) {
                            AppendEntityLivelinessCheck();
                            sb.AppendIndent(indent).AppendLine("var stash = GetStash(world);");
                            sb.AppendIndent(indent).AppendLine("success = stash.Has(entity);");
                            sb.AppendIndent(indent).AppendLine("if (success) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).AppendLine("return ref stash.Get(entity);");
                            }
                            sb.AppendIndent(indent).AppendLine("}");
                            sb.AppendIndent(indent).AppendLine("return ref stash.Empty;");
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                    }
                    
                    void AppendEntityLivelinessCheck() {
                        sb.AppendIndent(indent).AppendLine("var world = Scellecs.Morpeh.EntityExtensions.GetWorld(entity);");
                        sb.AppendIndent(indent).AppendLine("if (world == null || Scellecs.Morpeh.WorldEntityExtensions.IsDisposed(world, entity)) {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).Append("Scellecs.Morpeh.InvalidGetOperationException.ThrowDisposedEntity(entity, typeof(").Append(fullTypeName).AppendLine("));");
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                    }
                }
            }

            sb.AppendIndent(indent).AppendLine("}");
            
            ParentType.WriteClose(sb, indent, hierarchyDepth);

            if (component.TypeNamespace != null) {
                indent.Left();
                sb.AppendIndent(indent).AppendLine("}");
            }

            IndentSourcePool.Return(indent);

            return sb.ToStringAndReturn();
        }
    }
}