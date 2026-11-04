namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;
using Test.Namespace;

[EcsSystem]
[IncludeStash(typeof(TagComponent))]
[IncludeStash(typeof(Test.Namespace.GenericComponent<Test.Namespace.TagComponent>))]
[IncludeStash(typeof(GlobalNamespaceComponent))]
[IncludeStash(typeof(DisposableComponent))]
public partial class BasicSystem1 {
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