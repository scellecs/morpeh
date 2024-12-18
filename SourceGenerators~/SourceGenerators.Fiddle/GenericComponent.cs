namespace Test.Namespace;

using Scellecs.Morpeh;

[Component]
public partial struct GenericComponent<T> where T : struct {
    public T value;
}