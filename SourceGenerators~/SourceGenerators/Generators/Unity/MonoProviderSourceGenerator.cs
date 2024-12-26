namespace SourceGenerators.Generators.Unity {
    using System.Linq;
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
                (s, _) => s is ClassDeclarationSyntax,
                (ctx, _) => (ctx.TargetNode as ClassDeclarationSyntax, ctx.Attributes));

            context.RegisterSourceOutput(classes, static (spc, pair) => {
                var (typeDeclaration, monoProviderAttributes) = pair;
                if (typeDeclaration is null) {
                    return;
                }

                INamedTypeSymbol? monoProviderType = null;
                
                for (int i = 0, length = monoProviderAttributes.Length; i < length; i++) {
                    var attribute = monoProviderAttributes[i];
                    if (attribute.AttributeClass is null) {
                        continue;
                    }
                    
                    if (attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0] is { Kind: TypedConstantKind.Type, Value: INamedTypeSymbol positionalSymbol }) {
                        monoProviderType = positionalSymbol;
                    } else if (attribute.NamedArguments.Length > 0 && attribute.NamedArguments[0].Value is { Kind: TypedConstantKind.Type, Value: INamedTypeSymbol namedSymbol }) {
                        monoProviderType = namedSymbol;
                    }
                    
                    break;
                }
                
                if (monoProviderType is null) {
                    return;
                }

                var providerTypeName = monoProviderType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                // TODO: Check that this actually works.
                var isValidatable = monoProviderType.AllInterfaces.Any(static x => x.ToDisplayString() == "Scellecs.Morpeh.IValidatable");
                var isValidatableWithGameObject = monoProviderType.AllInterfaces.Any(static x => x.ToDisplayString() == "Scellecs.Morpeh.IValidatableWithGameObject");
                
                var providerStashSpecialization = MorpehComponentHelpersSemantic.GetStashSpecialization(monoProviderType);
                var isTag = providerStashSpecialization.variation == MorpehComponentHelpersSemantic.StashVariation.Tag;
                
                var typeName = typeDeclaration.Identifier.ToString();
                
                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                sb.AppendIndent(indent).AppendLine("using Sirenix.OdinInspector;");
                sb.AppendIndent(indent).AppendLine("using UnityEngine;");
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
                    .Append(" : Scellecs.Morpeh.Providers.EntityProvider ")
                    .AppendGenericConstraints(typeDeclaration)
                    .AppendLine(" {");

                using (indent.Scope()) {
                    if (!isTag) {
                        sb.AppendIndent(indent).AppendLine("[SerializeField]");
                        sb.AppendIndent(indent).AppendLine("[HideInInspector]");
                        sb.AppendIndent(indent).Append("private ").Append(providerTypeName).AppendLine(" serializedData;");
                    }
                    
                    sb.AppendIndent(indent).Append("private ").Append(providerStashSpecialization.type).AppendLine(" stash;");
                    
                    if (!isTag) {
                        sb.AppendLine().AppendLine();
                        sb.AppendIfDefine("UNITY_EDITOR");
                        sb.AppendIndent(indent).AppendLine("[PropertySpace]");
                        sb.AppendIndent(indent).AppendLine("[ShowInInspector]");
                        sb.AppendIndent(indent).AppendLine("[PropertyOrder(1)]");
                        sb.AppendIndent(indent).AppendLine("[HideLabel]");
                        sb.AppendIndent(indent).AppendLine("[InlineProperty]");
                        sb.AppendEndIfDefine();
                        sb.AppendIndent(indent).Append("private ").Append(providerTypeName).AppendLine(" Data {");
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
                    sb.AppendIndent(indent).AppendVisibility(monoProviderType).Append(" ").Append(providerStashSpecialization.type).AppendLine(" Stash {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).AppendLine("get {");
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).AppendLine("if (this.stash == null) {");
                            using (indent.Scope()) {
                                sb.AppendIndent(indent).Append("this.stash = ").Append(providerTypeName).AppendLine(".GetStash(World.Default);");
                            }
                            sb.AppendIndent(indent).AppendLine("}");
                            sb.AppendIndent(indent).AppendLine("return this.stash;");
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                    }
                    sb.AppendIndent(indent).AppendLine("}");

                    if (!isTag) {
                        sb.AppendLine().AppendLine();
                        sb.AppendIndent(indent).Append("public ref ").Append(providerTypeName).AppendLine(" GetSerializedData() => ref this.serializedData;");
                        
                        sb.AppendLine().AppendLine();
                        sb.AppendIndent(indent).Append("public ref ").Append(providerTypeName).AppendLine(" GetData() {");
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
                        sb.AppendIndent(indent).Append("public ref ").Append(providerTypeName).AppendLine(" GetData(out bool existOnEntity) {");
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
                        
                        sb.AppendLine().AppendLine();
                        sb.AppendIndent(indent).AppendLine("protected virtual void OnValidate() {");
                        using (indent.Scope()) {
                            if (isValidatable) {
                                sb.AppendIndent(indent).AppendLine("this.serializedData.OnValidate();");
                            }
                        
                            if (isValidatableWithGameObject) {
                                sb.AppendIndent(indent).Append("this.serializedData.OnValidate(this.gameObject);").AppendLine();
                            }
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                    }
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("protected sealed override void PreInitialize() {");
                    using (indent.Scope()) {
                        if (isTag) {
                            sb.AppendIndent(indent).AppendLine("this.Stash.Set(this.cachedEntity);");
                        }
                        else {
                            sb.AppendIndent(indent).AppendLine("this.Stash.Set(this.cachedEntity, this.serializedData);");
                        }
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
                sb.AppendEndNamespace(typeDeclaration, indent);
                
                spc.AddSource($"{typeDeclaration.Identifier.Text}.monoprovider_{typeDeclaration.GetStableFileCompliantHash()}.g.cs", sb.ToString());
                
                StringBuilderPool.Return(sb);
                IndentSourcePool.Return(indent);
            });
        }
    }
}