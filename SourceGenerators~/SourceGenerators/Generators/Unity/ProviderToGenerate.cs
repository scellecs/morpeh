namespace SourceGenerators.Generators.Unity {
    using Microsoft.CodeAnalysis;
    using MorpehHelpers.Semantic;
    using Utils.Collections;
    using Utils.Semantic;

    public record struct ProviderToGenerate(
        ParentType? Hierarchy,
        string TypeName,
        string? TypeNamespace,
        string GenericParams,
        string GenericConstraints,
        string ProviderTypeFullName,
        EquatableArray<StashRequirement> StashRequirements,
        Accessibility ProviderTypeVisibility,
        StashVariation StashVariation,
        Accessibility Visibility);
}