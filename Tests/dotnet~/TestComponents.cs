namespace Tests;

using Scellecs.Morpeh;

[Component]
public partial struct TagTest1 {
    
}

[Component]
public partial struct TagTest2 {
    
}

[Component]
public partial struct TagTest3 {
    
}

[Component]
public partial struct TagTest4 {
    
}

[Component]
public partial struct TagTest5 {
    
}

[Component]
public partial struct TagTest6 {
    
}

[Component]
public partial struct TagTest7 {
    
}

[Component]
public partial struct TagTest8 {
    
}

[Component]
public partial struct IntTest1 {
    public int value;
}

[Component]
public partial struct IntTest2 {
    public int value;
}

[Component]
public partial struct IntTest3 {
    public int value;
}

[Component]
public partial struct IntTest4 {
    public int value;
}

[Component]
public partial struct DisposableTest1 {
    public int    value;
    public Action<int> onDispose;
    
    public void Dispose() {
        this.onDispose?.Invoke(value);
    }
}

[Component]
public partial struct DisposableTest2 {
    public int         value;
    public Action<int> onDispose;
    
    public void Dispose() {
        this.onDispose?.Invoke(value);
    }
}

[Component]
public partial struct DisposableTest3 {
    public int         value;
    public Action<int> onDispose;
    
    public void Dispose() {
        this.onDispose?.Invoke(value);
    }
}

[Component]
public partial struct DisposableTest4 {
    public int         value;
    public Action<int> onDispose;
    
    public void Dispose() {
        this.onDispose?.Invoke(value);
    }
}

[Component]
public partial struct ManagedTest {
    public object value;
}

[Component]
public partial struct DisposableTest {
    public sealed class Handle {
        public bool value = true;
    }

    public Handle value;

    public void Dispose() {
        this.value.value = false;
    }
}