namespace SourceGenerators.Generators.Injection {
    public record struct InjectionField(
        string Name,
        string TypeName,
        string? GenericParams
    );
}