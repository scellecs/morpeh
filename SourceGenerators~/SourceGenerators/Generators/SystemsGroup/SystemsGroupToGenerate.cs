namespace SourceGenerators.Generators.SystemsGroup {
    using Microsoft.CodeAnalysis;
    using Utils.Collections;
    using Utils.Semantic;

    public record struct SystemsGroupToGenerate(
        ParentType? Hierarchy,
        string TypeName,
        string? TypeNamespace,
        EquatableArray<SystemsGroupField> Fields,
        TypeKind TypeKind,
        Accessibility Visibility,
        bool HasRegistrations,
        bool InlineUpdateMethods
    );
}