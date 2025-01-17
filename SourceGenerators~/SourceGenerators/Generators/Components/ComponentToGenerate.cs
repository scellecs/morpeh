namespace SourceGenerators.Generators.Components {
    using Microsoft.CodeAnalysis;
    using MorpehHelpers.Semantic;
    using Utils.Semantic;

    public record struct ComponentToGenerate(
        ParentType? Hierarchy,
        string TypeName,
        string? TypeNamespace,
        string GenericParams,
        string GenericConstraints,
        int InitialCapacity,
        StashVariation StashVariation,
        Accessibility Visibility
    );
}