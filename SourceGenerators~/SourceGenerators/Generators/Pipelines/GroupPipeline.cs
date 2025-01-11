namespace SourceGenerators.Generators.Pipelines {
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using MorpehHelpers.NonSemantic;
    using SystemsGroup;
    using SystemsGroupRunner;

    [Generator]
    public class GroupPipeline : IIncrementalGenerator {
        public void Initialize(IncrementalGeneratorInitializationContext context) {
            // TODO: Combine with loop information extraction.
        
            var groups = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.SYSTEMS_GROUP_FULL_NAME,
                (s, _) => s is TypeDeclarationSyntax,
                (ctx, _) => (ctx.TargetNode as TypeDeclarationSyntax, ctx.TargetSymbol as INamedTypeSymbol, ctx.Attributes));
        
            var runners = context.SyntaxProvider.ForAttributeWithMetadataName(
                MorpehAttributes.SYSTEMS_GROUP_RUNNER_FULL_NAME,
                (s, _) => s is ClassDeclarationSyntax,
                (ctx, _) => (ctx.TargetNode as ClassDeclarationSyntax, ctx.TargetSymbol as INamedTypeSymbol));

            context.RegisterSourceOutput(groups, static (spc, pair) => SystemsGroupSourceGenerator.Generate(spc, pair));
            context.RegisterSourceOutput(runners, static (spc, pair) => SystemsGroupRunnerSourceGenerator.Generate(spc, pair));
        }
    }
}