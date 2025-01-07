namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

public abstract class BaseInjectionHierarchyClass {
    [Injectable]
    private IDisposable _disposable1;
}

public partial class InjectionHierarchyClass : BaseInjectionHierarchyClass {
    [Injectable]
    private IDisposable _disposable2;
}