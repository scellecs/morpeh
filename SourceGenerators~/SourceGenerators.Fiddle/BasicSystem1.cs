namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;
using Test.Namespace;

[System]
public partial class BasicSystem1 {
    [Require(typeof(TagComponent))]
    private TagStash _tag;
    
    [Require(typeof(GenericComponent<int>))]
    private Stash<GenericComponent<int>> _generic;
    
    [Require(typeof(GlobalNamespaceComponent))]
    private TagStash _globalNamespace;
    
    [Require(typeof(DisposableComponent))]
    private StashD<DisposableComponent> _disposable;
    
    public bool IsEnabled() => throw new NotImplementedException();

    public void OnAwake() {
        throw new NotImplementedException();
    }
    
    public void OnUpdate(float deltaTime) {
        throw new NotImplementedException();
    }
    
    public void Dispose() {
        throw new NotImplementedException();
    }
}