namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

[Injectable]
public abstract partial class BaseInjectionHierarchyClass {
    [Injectable]
    private IDisposable _disposable_base1;
    
    [Injectable]
    private IDisposable _disposable_base2;
}

[Injectable]
public partial class InjectionHierarchyClass : BaseInjectionHierarchyClass {
    [Injectable]
    private IDisposable _disposable;
}

public partial class OuterScopeForParentInjection {
    [Injectable]
    public partial class InjectionUpperHierarchyClass : InjectionHierarchyClass {
        [Injectable]
        private IDisposable _disposable_upper;
    }
}

public class InjectionMiddlewareClass : OuterScopeForParentInjection.InjectionUpperHierarchyClass {
    
}

public partial class OuterScopeForInjection {
    [Injectable]
    public partial class InjectionAfterMiddlewareClass : InjectionMiddlewareClass {
        [Injectable]
        private IDisposable _disposable_after_middleware;
    }
}