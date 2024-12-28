namespace Tests;

using Scellecs.Morpeh;

[EcsComponent]
public partial struct TagTest1 {
    
}

[EcsComponent]
public partial struct TagTest2 {
    
}

[EcsComponent]
public partial struct TagTest3 {
    
}

[EcsComponent]
public partial struct TagTest4 {
    
}

[EcsComponent]
public partial struct TagTest5 {
    
}

[EcsComponent]
public partial struct TagTest6 {
    
}

[EcsComponent]
public partial struct TagTest7 {
    
}

[EcsComponent]
public partial struct TagTest8 {
    
}

[EcsComponent]
public partial struct IntTest1 {
    public int value;
}

[EcsComponent]
public partial struct IntTest2 {
    public int value;
}

[EcsComponent]
public partial struct IntTest3 {
    public int value;
}

[EcsComponent]
public partial struct IntTest4 {
    public int value;
}

[EcsComponent]
public partial struct DisposableTest1 {
    public int    value;
    public Action<int> onDispose;
    
    public void Dispose() {
        this.onDispose?.Invoke(value);
    }
}

[EcsComponent]
public partial struct DisposableTest2 {
    public int         value;
    public Action<int> onDispose;
    
    public void Dispose() {
        this.onDispose?.Invoke(value);
    }
}

[EcsComponent]
public partial struct DisposableTest3 {
    public int         value;
    public Action<int> onDispose;
    
    public void Dispose() {
        this.onDispose?.Invoke(value);
    }
}

[EcsComponent]
public partial struct DisposableTest4 {
    public int         value;
    public Action<int> onDispose;
    
    public void Dispose() {
        this.onDispose?.Invoke(value);
    }
}

[EcsComponent]
public partial struct ManagedTest {
    public object value;
}

[EcsComponent]
public partial struct DisposableTest {
    public sealed class Handle {
        public bool value = true;
    }

    public Handle value;

    public void Dispose() {
        this.value.value = false;
    }
}