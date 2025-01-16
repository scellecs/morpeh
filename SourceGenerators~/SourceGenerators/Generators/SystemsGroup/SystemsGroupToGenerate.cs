namespace SourceGenerators.Generators.SystemsGroup {
    using Microsoft.CodeAnalysis;
    using Utils.Collections;

    public record struct SystemsGroupToGenerate(
        string TypeName,
        string? TypeNamespace,
        string GenericParams,
        string GenericConstraints,
        EquatableArray<SystemsGroupField> Fields,
        TypeKind TypeKind,
        Accessibility Visibility,
        bool InlineUpdateMethods
    );
}