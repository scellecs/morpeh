namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;
using Test.Namespace;

[System]
[Require(typeof(TagComponent))]
[Require(typeof(Test.Namespace.GenericComponent<Test.Namespace.TagComponent>))]
[Require(typeof(GlobalNamespaceComponent))]
[Require(typeof(DisposableComponent))]
public partial class BasicSystem1 {
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