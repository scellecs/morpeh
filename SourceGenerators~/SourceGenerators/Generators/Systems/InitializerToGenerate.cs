namespace SourceGenerators.Generators.Systems {
    using Microsoft.CodeAnalysis;
    using MorpehHelpers.Semantic;
    using Utils.Collections;

    public record struct InitializerToGenerate(
        string TypeName,
        string? TypeNamespace,
        string GenericParams,
        string GenericConstraints,
        EquatableArray<StashRequirement> StashRequirements,
        TypeKind TypeKind,
        Accessibility Visibility);
}