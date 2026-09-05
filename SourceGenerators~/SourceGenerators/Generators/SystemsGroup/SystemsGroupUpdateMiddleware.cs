namespace SourceGenerators.Generators.SystemsGroup {
    public record struct SystemsGroupUpdateMiddleware(
        string FullTypeName,
        int Priority,
        bool IsDisposable
    );
}
