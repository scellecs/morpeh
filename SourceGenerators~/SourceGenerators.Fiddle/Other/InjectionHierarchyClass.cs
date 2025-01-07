namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

public abstract class BaseInjectionHierarchyClass {
    // Will not be generated as it's private
    [Injectable]
    private IDisposable _disposable_base1;
    
    // Should be generated
    [Injectable]
    protected IDisposable _disposable_base2;
}

public partial class InjectionHierarchyClass : BaseInjectionHierarchyClass {
    [Injectable]
    protected IDisposable _disposable;
}

// TODO: Add support for multiple inheritance?
public partial class InjectionUpperHierarchyClass : InjectionHierarchyClass {
    [Injectable]
    private IDisposable _disposable_upper;
}