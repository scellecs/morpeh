namespace SourceGenerators.Generators.Injection {
    using Microsoft.CodeAnalysis;
    using Utils.Collections;
    using Utils.Semantic;

    public record struct InjectionToGenerate(
        ParentType? Hierarchy,
        string TypeName,
        string? TypeNamespace,
        string GenericParams,
        string GenericConstraints,
        EquatableArray<InjectionField> Fields,
        TypeKind TypeKind,
        Accessibility Visibility,
        bool HasInjectionsInParents
    );
}