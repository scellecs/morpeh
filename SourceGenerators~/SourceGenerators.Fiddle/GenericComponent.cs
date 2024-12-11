namespace Test.Namespace;

using Scellecs.Morpeh;

public struct TestComponent<T> : IComponent where T : struct {
    public T value;
}