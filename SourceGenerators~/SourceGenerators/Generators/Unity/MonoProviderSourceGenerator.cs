namespace SourceGenerators.Generators.Unity {
    using System;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Utils.NonSemantic;
    using Utils.Pools;
    using Utils.Semantic;

    [Generator]
    public class MonoProviderSourceGenerator : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.MONO_PROVIDER_FULL_NAME,
                predicate: static (s, _) => s is ClassDeclarationSyntax cds && cds.Parent is not TypeDeclarationSyntax,
                transform: static (ctx, ct) => ExtractTypesToGenerate(ctx, ct))
                .WithTrackingName(TrackingNames.FIRST_PASS)
                .Where(candidate => candidate is not null)
                .Select(static (candidate, _) => candidate!.Value)
                .WithTrackingName(TrackingNames.REMOVE_NULL_PASS);

            context.RegisterSourceOutput(classes, static (spc, provider) => {
                var specializationType = MorpehComponentHelpersSemantic.GetStashSpecializationType(provider.StashVariation, provider.ProviderTypeFullName);
                var typeName = provider.TypeName;
                
                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                sb.AppendIndent(indent).AppendLine("using Sirenix.OdinInspector;");
                sb.AppendIndent(indent).AppendLine("using UnityEngine;");
                sb.AppendIndent(indent).AppendLine("using Scellecs.Morpeh;");
                
                if (provider.TypeNamespace != null) {
                    sb.AppendIndent(indent).Append("namespace ").Append(provider.TypeNamespace).AppendLine(" {");
                    indent.Right();
                }
                
                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent)
                    .Append(Types.GetVisibilityModifierString(provider.Visibility))
                    .Append(" partial class ")
                    .Append(typeName)
                    .Append(provider.GenericParams)
                    .Append(" : Scellecs.Morpeh.Providers.EntityProvider ")
                    .Append(provider.GenericConstraints)
                    .AppendLine(" {");

                using (indent.Scope()) {
                    if (provider.StashVariation != StashVariation.Tag) {
                        sb.AppendIndent(indent).AppendLine("[SerializeField]");
                        sb.AppendIndent(indent).AppendLine("[HideInInspector]");
                        sb.AppendIndent(indent).Append("private ").Append(provider.ProviderTypeFullName).AppendLine(" serializedData;");
                    }
                    
                    sb.AppendIndent(indent).Append("private ").Append(specializationType).AppendLine(" stash;");
                    
                    if (provider.StashVariation != StashVariation.Tag) {
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
                    sb.AppendIndent(indent).Append(Types.GetVisibilityModifierString(provider.ProviderTypeVisibility)).Append(" ").Append(specializationType).AppendLine(" Stash {");
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

                    if (provider.StashVariation != StashVariation.Tag) {
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
                        sb.AppendIndent(indent).AppendLine(provider.StashVariation == StashVariation.Tag
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
                
                if (provider.TypeNamespace != null) {
                    indent.Left();
                    sb.AppendIndent(indent).AppendLine("}");
                }
                
                spc.AddSource($"{typeName}.monoprovider_{Guid.NewGuid():N}.g.cs", sb.ToStringAndReturn());
                
                IndentSourcePool.Return(indent);
            });
        }

        private static ProviderToGenerate? ExtractTypesToGenerate(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
            ct.ThrowIfCancellationRequested();
            
            if (ctx.TargetNode is not ClassDeclarationSyntax syntaxNode) {
                return null;
            }
            
            if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol) {
                return null;
            }
            
            var attribute = ctx.Attributes.First();
            INamedTypeSymbol? monoProviderType = null;
            
            if (attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0] is { Kind: TypedConstantKind.Type, Value: INamedTypeSymbol symbol }) {
                monoProviderType = symbol;
            }
                
            if (monoProviderType is null) {
                return null;
            }
            
            return new ProviderToGenerate(
                TypeName: syntaxNode.Identifier.ToString(),
                TypeNamespace: typeSymbol.ContainingNamespace.IsGlobalNamespace ? null : typeSymbol.ContainingNamespace.ToDisplayString(),
                GenericParams: syntaxNode.TypeParameterList?.ToString() ?? string.Empty,
                GenericConstraints: typeSymbol.IsGenericType ? StringBuilderPool.Get().AppendGenericConstraints(typeSymbol).ToStringAndReturn() : string.Empty,
                ProviderTypeFullName: monoProviderType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                ProviderTypeVisibility: monoProviderType.DeclaredAccessibility,
                StashVariation: MorpehComponentHelpersSemantic.GetStashVariation(monoProviderType),
                Visibility: typeSymbol.DeclaredAccessibility);
        }

        private record struct ProviderToGenerate(
            string TypeName,
            string? TypeNamespace,
            string GenericParams,
            string GenericConstraints,
            string ProviderTypeFullName,
            Accessibility ProviderTypeVisibility,
            StashVariation StashVariation,
            Accessibility Visibility);
    }
}