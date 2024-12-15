namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;
using Test.Namespace;

[Initializer]
public partial class BasicInitializer1 {
    [Require(typeof(TagComponent))]
    private TagStash _tag;
    
    [Require(typeof(GenericComponent<int>))]
    private Stash<GenericComponent<int>> _generic;
    
    [Require(typeof(GlobalNamespaceComponent))]
    private TagStash _globalNamespace;
    
    [Require(typeof(DisposableComponent))]
    private StashD<DisposableComponent> _disposable;

    public void OnAwake() {
        throw new NotImplementedException();
    }
    
    public void Dispose() {
        throw new NotImplementedException();
    }
}