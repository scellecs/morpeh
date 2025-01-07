namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

public abstract class BaseInjectionHierarchyClass {
    // Will not be generated as it's private
    [Inject]
    private IDisposable _disposable_base1;
    
    // Should be generated
    [Inject]
    protected IDisposable _disposable_base2;
}

public partial class InjectionHierarchyClass : BaseInjectionHierarchyClass {
    [Inject]
    protected IDisposable _disposable;
}

// TODO: Add support for multiple inheritance?
public partial class InjectionUpperHierarchyClass : InjectionHierarchyClass {
    [Inject]
    private IDisposable _disposable_upper;
}