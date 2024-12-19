namespace Test.Namespace;

using Scellecs.Morpeh;

[Component(initialCapacity: 64)]
internal partial struct DisposableComponent {
    public int value;
    
    public void Dispose() {
        throw new System.NotImplementedException();
    }
}