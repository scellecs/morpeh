namespace Test.Namespace;

using Scellecs.Morpeh;

[EcsComponent]
public partial struct GenericComponent<T> where T : unmanaged {
    public T value;
}