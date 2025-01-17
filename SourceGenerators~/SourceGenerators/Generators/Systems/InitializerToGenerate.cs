namespace SourceGenerators.Generators.Systems {
    using Microsoft.CodeAnalysis;
    using MorpehHelpers.Semantic;
    using Utils.Collections;
    using Utils.Semantic;

    public record struct InitializerToGenerate(
        ParentType? Hierarchy,
        string TypeName,
        string? TypeNamespace,
        string GenericParams,
        string GenericConstraints,
        EquatableArray<StashRequirement> StashRequirements,
        TypeKind TypeKind,
        Accessibility Visibility
    );
}