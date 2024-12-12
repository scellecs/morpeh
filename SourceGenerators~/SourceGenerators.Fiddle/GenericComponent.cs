namespace Test.Namespace;

using Scellecs.Morpeh;

public struct GenericComponent<T> : IComponent where T : struct {
    public T value;
}