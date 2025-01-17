namespace SourceGenerators.Generators.Injection {
    public record struct GenericResolver(
        string BaseTypeName,
        string ResolverTypeName
    );
}