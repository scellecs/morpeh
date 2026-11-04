namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;
using Test.Namespace;

[EcsSystem]
[IncludeStash(typeof(TagComponent))]
[IncludeStash(typeof(GenericComponent<int>))]
[IncludeStash(typeof(GenericComponent<GenericComponent<int>>))]
[IncludeStash(typeof(GlobalNamespaceComponent))]
[IncludeStash(typeof(DisposableComponent))]
public partial class BasicGenericSystem<T> where T : struct {
    public World World { get; }
    
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