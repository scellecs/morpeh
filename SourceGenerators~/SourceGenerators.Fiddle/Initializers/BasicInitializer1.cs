namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;
using Test.Namespace;

[EcsInitializer]
[Injectable]
[IncludeStash(typeof(TagComponent))]
[IncludeStash(typeof(GenericComponent<int>))]
[IncludeStash(typeof(GlobalNamespaceComponent), customFieldName: "_globalTest")]
[IncludeStash(typeof(DisposableComponent))]
public partial class BasicInitializer1 {
    [Injectable]
    private BasicDisposableClass _basicDisposableClass1;
    
    [Injectable]
    private IDisposable _basicDisposableClass2;
    
    [Injectable]
    private SomeGenericClass<int> _someGenericClass;
    
    [Injectable]
    private SomeComplexGenericClass<int, string> _someComplexGenericClass;

    public World World { get; }

    public void OnAwake() {
        this._tagComponent.Has(default);
        this._genericComponent_Int32.Has(default);
        this._globalTest.Has(default);
        this._disposableComponent.Has(default);
    }
    
    public void Dispose() {
        throw new NotImplementedException();
    }
    
    public void OnUpdate(float deltaTime) {
        throw new NotImplementedException();
    }
}