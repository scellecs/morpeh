namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

[Injectable]
public abstract partial class BaseInjectionHierarchyClass {
    [Inject]
    private IDisposable _disposable_base1;
    
    [Inject]
    private IDisposable _disposable_base2;
}

[Injectable]
public partial class InjectionHierarchyClass : BaseInjectionHierarchyClass {
    [Inject]
    private IDisposable _disposable;
}

[Injectable]
public partial class InjectionUpperHierarchyClass : InjectionHierarchyClass {
    [Inject]
    private IDisposable _disposable_upper;
}

public class InjectionMiddlewareClass : InjectionUpperHierarchyClass {
    
}

[Injectable]
public partial class InjectionAfterMiddlewareClass : InjectionMiddlewareClass {
    [Inject]
    private IDisposable _disposable_after_middleware;
}