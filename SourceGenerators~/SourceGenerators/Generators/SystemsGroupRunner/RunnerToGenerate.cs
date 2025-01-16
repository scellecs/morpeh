namespace SourceGenerators.Generators.SystemsGroupRunner {
    using Microsoft.CodeAnalysis;
    using Utils.Collections;
    using Utils.Semantic;

    public record struct RunnerToGenerate(
        ParentType? Hierarchy,
        string TypeName,
        string? TypeNamespace,
        EquatableArray<RunnerField> Fields,
        TypeKind TypeKind,
        Accessibility Visibility
    );
}