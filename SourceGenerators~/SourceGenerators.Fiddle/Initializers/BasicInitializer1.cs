namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;
using Test.Namespace;

[EcsInitializer]
[Injectable]
[IncludeStash(typeof(TagComponent))]
[IncludeStash(typeof(GenericComponent<int>))]
[IncludeStash(typeof(GlobalNamespaceComponent))]
[IncludeStash(typeof(DisposableComponent))]
public partial class BasicInitializer1 {
    [Inject]
    private BasicDisposableClass _basicDisposableClass1;
    
    [Inject]
    private IDisposable _basicDisposableClass2;
    
    [Inject]
    private SomeGenericClass<int> _someGenericClass;
    
    [Inject]
    private SomeComplexGenericClass<int, string> _someComplexGenericClass;
    
    public void OnAwake() {
        throw new NotImplementedException();
    }
    
    public void Dispose() {
        throw new NotImplementedException();
    }
    
    public void OnUpdate(float deltaTime) {
        throw new NotImplementedException();
    }
}