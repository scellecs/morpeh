namespace Test.Namespace;

using Scellecs.Morpeh;

public struct DisposableComponent : IComponent, System.IDisposable {
    public int value;
    
    public void Dispose() {
        throw new System.NotImplementedException();
    }
}