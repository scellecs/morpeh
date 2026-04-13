namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

[EcsSystemUpdateMiddleware(priority: -100)]
public readonly struct ConsoleWriteLineSystemUpdateMiddleware {
    private readonly string systemName;

    public ConsoleWriteLineSystemUpdateMiddleware(string systemName) => this.systemName = systemName;
    public void Begin() => Console.WriteLine($"Begin {this.systemName}");
    public void End() => Console.WriteLine($"End {this.systemName}");
}