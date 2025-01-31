namespace SourceGenerators.Generators.SystemsGroup {
    public record struct SystemsGroupField(
        string Name,
        string TypeName,
        string? RegisterAs,
        SystemsGroupFieldKind FieldKind,
        bool IsInjectable
    );
}