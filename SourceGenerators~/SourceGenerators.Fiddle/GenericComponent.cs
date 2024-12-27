namespace Test.Namespace;

using Scellecs.Morpeh;

[Component]
public partial struct GenericComponent<T> where T : unmanaged {
    public T value;
}