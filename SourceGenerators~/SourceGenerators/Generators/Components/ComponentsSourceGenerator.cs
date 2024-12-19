namespace SourceGenerators.Generators.ComponentsMetadata {
    using System.Runtime.CompilerServices;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Diagnostics;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Utils.NonSemantic;
    using Utils.Pools;

    [Generator]
    public class ComponentsSourceGenerator : IIncrementalGenerator {
        private const int DEFAULT_STASH_CAPACITY = 16;

        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var structs = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.COMPONENT_FULL_NAME,
                (s, _) => s is StructDeclarationSyntax,
                (ctx, _) => (ctx.TargetNode as StructDeclarationSyntax, ctx.TargetSymbol as ITypeSymbol, ctx.Attributes));

            context.RegisterSourceOutput(structs, static (spc, pair) => {
                var (structDeclaration, typeSymbol, componentAttributes) = pair;
                
                if (structDeclaration is null || typeSymbol is null) {
                    return;
                }
                
                if (structDeclaration.IsDeclaredInsideAnotherType()) {
                    Errors.ReportNestedDeclaration(spc, structDeclaration);
                    return;
                }

                var typeName = structDeclaration.Identifier.ToString();
                
                string genericParams;
                using (var scoped = StringBuilderPool.GetScoped()) {
                    genericParams = scoped.StringBuilder.AppendGenericParams(structDeclaration).ToString();
                }
                
                string genericConstraints;
                using (var scoped = StringBuilderPool.GetScoped()) {
                    genericConstraints = scoped.StringBuilder.AppendGenericConstraints(structDeclaration).ToString();
                }
                
                var initialCapacity = -1;
                
                for (int i = 0, length = componentAttributes.Length; i < length; i++) {
                    var attribute = componentAttributes[i];
                    var args = attribute.ConstructorArguments;
                    if (args.Length >= 1 && args[0].Value is int capacity) {
                        initialCapacity = capacity;
                    }
                }
                
                var specialization = MorpehComponentHelpersSemantic.GetStashSpecialization(typeSymbol);
            
                var sb     = StringBuilderPool.Get();
                var indent = IndentSourcePool.Get();
            
                sb.AppendIndent(indent).AppendLine("using Scellecs.Morpeh;");
                
                sb.AppendBeginNamespace(structDeclaration, indent).AppendLine();
                
                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent).AppendVisibility(structDeclaration)
                    .Append(" partial struct ")
                    .Append(typeName)
                    .Append(genericParams)
                    .Append(" : ")
                    .Append(specialization.constraintInterface)
                    .Append(' ')
                    .Append(genericConstraints)
                    .AppendLine(" {");
                
                using (indent.Scope()) {
                    sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                    sb.AppendIndent(indent).Append("public static ").Append(specialization.type).Append(" GetStash(World world) => world.").Append(specialization.getStashMethod)
                        .Append("(")
                        .Append("capacity: ").Append(initialCapacity)
                        .AppendLine(");");
                }
                sb.AppendIndent(indent).AppendLine("}");
                
                sb.AppendEndNamespace(structDeclaration, indent).AppendLine();
                
                spc.AddSource($"{structDeclaration.Identifier.Text}.component_{structDeclaration.GetStableFileCompliantHash()}.g.cs", sb.ToString());
                
                StringBuilderPool.Return(sb);
                IndentSourcePool.Return(indent);
            });
        }
    }
}