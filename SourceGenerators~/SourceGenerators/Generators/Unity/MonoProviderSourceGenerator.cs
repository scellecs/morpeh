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
                (ctx, _) => (ctx.TargetNode as ClassDeclarationSyntax, ctx.SemanticModel, ctx.Attributes));

            context.RegisterSourceOutput(classes, static (spc, pair) => {
                var (typeDeclaration, semanticModel, monoProviderAttributes) = pair;
                if (typeDeclaration is null) {
                    return;
                }

                INamedTypeSymbol? monoProviderType = null;
                
                for (int i = 0, length = monoProviderAttributes.Length; i < length; i++) {
                    var attribute = monoProviderAttributes[i];
                    if (attribute.AttributeClass is null) {
                        continue;
                    }
                    
                    if (attribute.ConstructorArguments.Length > 0 && attribute.ConstructorArguments[0] is { Kind: TypedConstantKind.Type, Value: INamedTypeSymbol injectAsPositionalSymbol }) {
                        monoProviderType = injectAsPositionalSymbol;
                    } else if (attribute.NamedArguments.Length > 0 && attribute.NamedArguments[0].Value is { Kind: TypedConstantKind.Type, Value: INamedTypeSymbol injectAsNamedSymbol }) {
                        monoProviderType = injectAsNamedSymbol;
                    }
                    
                    break;
                }
                
                if (monoProviderType is null) {
                    return;
                }

                string providerTypeName;
                using (var scoped = StringBuilderPool.GetScoped()) {
                    scoped.StringBuilder.Append(monoProviderType.Name);
                    scoped.StringBuilder.AppendGenericParams(monoProviderType);
                    providerTypeName = scoped.StringBuilder.ToString();
                }

                var isValidatable = monoProviderType.AllInterfaces.Any(x => x.Name == "IValidatable");
                var isValidatableWithGameObject = monoProviderType.AllInterfaces.Any(x => x.Name == "IValidatableWithGameObject");
                
                var providerStashSpecialization = MorpehComponentHelpersSemantic.GetStashSpecialization(monoProviderType);
                
                var typeName = typeDeclaration.Identifier.ToString();
                
                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
                
                sb.AppendUsings(typeDeclaration).AppendLine();
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
                    sb.AppendIndent(indent).AppendLine("[SerializeField]");
                    sb.AppendIndent(indent).AppendLine("[HideInInspector]");
                    sb.AppendIndent(indent).Append("private ").Append(providerTypeName).AppendLine(" serializedData;");
                    sb.AppendIndent(indent).Append("private ").Append(providerStashSpecialization.type).AppendLine(" stash;");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIfDefine("UNITY_EDITOR");
                    // TODO: Can be replaced with constant string
                    sb.AppendIndent(indent).Append("private string typeName = typeof(").Append(providerTypeName).AppendLine(").Name;");
                    sb.AppendLine();
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
                            sb.AppendIndent(indent).AppendLine("if (this.cachedEntity.IsNullOrDisposed() == false) {");
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
                            sb.AppendIndent(indent).AppendLine("if (this.cachedEntity.IsNullOrDisposed() == false) {");
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
                    
                    sb.AppendLine().AppendLine();
                    // TODO: If component is internal, stash accessor should be internal too?
                    sb.AppendIndent(indent).Append("public ").Append(providerStashSpecialization.type).AppendLine(" Stash {");
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
                    
                    sb.AppendLine().AppendLine();
                    // TODO: Won't work with tag components. Should it not be generated?
                    sb.AppendIndent(indent).Append("public ref ").Append(providerTypeName).AppendLine(" GetSerializedData() => ref this.serializedData;");
                    
                    sb.AppendLine().AppendLine();
                    // TODO: Same issue as with GetSerializedData
                    sb.AppendIndent(indent).Append("public ref ").Append(providerTypeName).AppendLine(" GetData() {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).Append("var ent = this.Entity;").AppendLine();
                        sb.AppendIndent(indent).Append("if (ent.IsNullOrDisposed() == false) {").AppendLine();
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
                    // TODO: Same issue as with GetSerializedData
                    sb.AppendIndent(indent).Append("public ref ").Append(providerTypeName).AppendLine(" GetData(out bool existOnEntity) {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).Append("if (this.cachedEntity.IsNullOrDisposed() == false) {").AppendLine();
                        using (indent.Scope()) {
                            sb.AppendIndent(indent).Append("return ref this.Stash.Get(this.cachedEntity, out existOnEntity);").AppendLine();
                        }
                        sb.AppendIndent(indent).AppendLine("}");
                        sb.AppendIndent(indent).AppendLine("existOnEntity = false;");
                        sb.AppendIndent(indent).AppendLine("return ref this.serializedData;");
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    // TODO: Same issue as with GetSerializedData
                    sb.AppendIndent(indent).AppendLine("protected virtual void OnValidate() {");
                    using (indent.Scope()) {
                        if (isValidatable) {
                            sb.AppendIndent(indent).AppendLine("var validatable = this.serializedData as IValidatable;");
                            sb.AppendIndent(indent).AppendLine("validatable.OnValidate();");
                            sb.AppendIndent(indent).Append("this.serializedData = (").Append(providerTypeName).Append(")validatable;").AppendLine();
                        }
                        
                        if (isValidatableWithGameObject) {
                            sb.AppendIndent(indent).AppendLine("var validatableWithGameObject = this.serializedData as IValidatableWithGameObject;");
                            sb.AppendIndent(indent).Append("validatableWithGameObject.OnValidate(this.gameObject);").AppendLine();
                            sb.AppendIndent(indent).Append("this.serializedData = (").Append(providerTypeName).Append(")validatableWithGameObject;").AppendLine();
                        }
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("protected sealed override void PreInitialize() {");
                    using (indent.Scope()) {
                        // TODO: For tag components, this should just set without any serialized data
                        sb.AppendIndent(indent).Append("this.Stash.Set(this.cachedEntity, this.serializedData);").AppendLine();
                    }
                    sb.AppendIndent(indent).AppendLine("}");
                    
                    sb.AppendLine().AppendLine();
                    sb.AppendIndent(indent).AppendLine("protected sealed override void PreDeinitialize() {");
                    using (indent.Scope()) {
                        sb.AppendIndent(indent).AppendLine("var ent = this.Entity;");
                        sb.AppendIndent(indent).AppendLine("if (ent.IsNullOrDisposed() == false) {");
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