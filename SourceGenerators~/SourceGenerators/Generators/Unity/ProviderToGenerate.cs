namespace SourceGenerators.Generators.Unity {
    using Microsoft.CodeAnalysis;
    using MorpehHelpers.Semantic;
    using Utils.Semantic;

    public record struct ProviderToGenerate(
        ParentType? Hierarchy,
        string TypeName,
        string? TypeNamespace,
        string GenericParams,
        string GenericConstraints,
        string ProviderTypeFullName,
        Accessibility ProviderTypeVisibility,
        StashVariation StashVariation,
        Accessibility Visibility);
}