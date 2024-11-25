using Scellecs.Morpeh;

namespace Tests;

public struct Test1 : IComponent { }
public struct Test2 : IComponent { }
public struct Test3 : IComponent { }
public struct Test4 : IComponent { }

public struct IntTest1 : IComponent { public int value; }

public struct ManagedTest : IComponent {
    public object value;
}

public struct DisposableTest : IComponent, IDisposable {
    public sealed class Handle {
        public bool value = true;
    }

    public Handle value;

    public void Dispose() {
        this.value.value = false;
    }
}