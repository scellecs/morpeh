namespace SourceGenerators.Generators.SystemsGroupRunner {
    using Microsoft.CodeAnalysis;
    using Utils.Collections;

    public record struct RunnerToGenerate(
        string TypeName,
        string? TypeNamespace,
        EquatableArray<RunnerField> Fields,
        TypeKind TypeKind,
        Accessibility Visibility
    );
}