namespace SourceGenerators.Generators.Components {
    using Microsoft.CodeAnalysis;
    using MorpehHelpers.Semantic;

    public record struct ComponentToGenerate(
        string TypeName,
        string? TypeNamespace,
        string GenericParams,
        string GenericConstraints,
        int InitialCapacity,
        StashVariation StashVariation,
        Accessibility Visibility);
}