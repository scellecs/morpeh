﻿namespace SourceGenerators.Generators.Components {
    using System.Runtime.CompilerServices;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Diagnostics;
    using MorpehHelpers.NonSemantic;
    using MorpehHelpers.Semantic;
    using Utils.NonSemantic;
    using Utils.Pools;

    [Generator]
    public class ComponentSourceGenerator : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            var structs = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.COMPONENT_FULL_NAME,
                (s, _) => s is StructDeclarationSyntax,
                (ctx, _) => (ctx.TargetNode as StructDeclarationSyntax, ctx.TargetSymbol as ITypeSymbol, ctx.Attributes));

            context.RegisterSourceOutput(structs, static (spc, pair) => {
                var (typeDeclaration, typeSymbol, componentAttributes) = pair;
                
                if (typeDeclaration is null || typeSymbol is null) {
                    return;
                }
                
                if (!RunDiagnostics(spc, typeDeclaration)) {
                    return;
                }

                var typeName = typeDeclaration.Identifier.ToString();
                
                var initialCapacity = 16;
                
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
                
                sb.AppendBeginNamespace(typeDeclaration, indent).AppendLine();
                
                sb.AppendIl2CppAttributes(indent);
                sb.AppendIndent(indent).AppendVisibility(typeDeclaration)
                    .Append(" partial struct ")
                    .Append(typeName)
                    .AppendGenericParams(typeDeclaration)
                    .Append(" : ")
                    .Append(specialization.constraintInterface)
                    .Append(' ')
                    .AppendGenericConstraints(typeDeclaration)
                    .AppendLine(" {");
                
                using (indent.Scope()) {
                    sb.AppendInlining(MethodImplOptions.AggressiveInlining, indent);
                    sb.AppendIndent(indent).Append("public static ").Append(specialization.type).Append(" GetStash(World world) => world.").Append(specialization.getStashMethod)
                        .Append("(")
                        .Append("capacity: ").Append(initialCapacity)
                        .AppendLine(");");
                }
                sb.AppendIndent(indent).AppendLine("}");
                
                sb.AppendEndNamespace(typeDeclaration, indent).AppendLine();
                
                spc.AddSource($"{typeDeclaration.Identifier.Text}.component_{typeDeclaration.GetStableFileCompliantHash()}.g.cs", sb.ToString());
                
                StringBuilderPool.Return(sb);
                IndentSourcePool.Return(indent);
            });
        }

        private static bool RunDiagnostics(SourceProductionContext spc, StructDeclarationSyntax typeDeclaration) {
            var success = true;

            if (typeDeclaration.IsDeclaredInsideAnotherType()) {
                Errors.ReportNestedDeclaration(spc, typeDeclaration);
                success = false;
            }

            return success;
        }
    }
}