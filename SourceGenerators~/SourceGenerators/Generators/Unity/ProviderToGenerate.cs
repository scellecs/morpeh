namespace SourceGenerators.Generators.Unity {
    using Microsoft.CodeAnalysis;
    using MorpehHelpers.Semantic;

    public record struct ProviderToGenerate(
        string TypeName,
        string? TypeNamespace,
        string GenericParams,
        string GenericConstraints,
        string ProviderTypeFullName,
        Accessibility ProviderTypeVisibility,
        StashVariation StashVariation,
        Accessibility Visibility);
}