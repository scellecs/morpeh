namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

[Injectable]
public abstract partial class BaseInjectionHierarchyClass {
    // Will not be generated as it's private
    [Inject]
    private IDisposable _disposable_base1;
    
    // Should be generated
    [Inject]
    protected IDisposable _disposable_base2;
}

[Injectable]
public partial class InjectionHierarchyClass : BaseInjectionHierarchyClass {
    [Inject]
    protected IDisposable _disposable;
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