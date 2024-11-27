using Scellecs.Morpeh;

namespace Tests;
#pragma warning disable 8618
public abstract class BaseTestSystem
{
    public bool throwException = false;
    public int updateCount = 0;
    public bool isDisposed = false;
}

public class TestUpdateSystem : BaseTestSystem, ISystem {
    public World World { get; set; }

    public void Dispose() {
        isDisposed = true;
    }

    public void OnAwake() {
    
    }

    public void OnUpdate(float deltaTime) {
        this.updateCount++;

        if (this.throwException) {
            this.throwException = false;
            throw new ArgumentException();
        }
    }
}

public class TestFixedSystem : BaseTestSystem, IFixedSystem {
    public World World { get; set; }

    public void Dispose() { 
        isDisposed = true;
    }

    public void OnAwake() { 
    
    }

    public void OnUpdate(float deltaTime) {
        this.updateCount++;

        if (throwException) {
            this.throwException = false;
            throw new ArgumentException();
        }
    }
}

public class TestLateSystem : BaseTestSystem, ILateSystem {
    public World World { get; set; }

    public void Dispose() {
        isDisposed = true;
    }

    public void OnAwake() { 
    
    }

    public void OnUpdate(float deltaTime) {
        this.updateCount++;

        if (throwException) {
            this.throwException = false;
            throw new ArgumentException();
        }
    }
}

public class TestCleanupSystem : BaseTestSystem, ICleanupSystem {
    public World World { get; set; }

    public void Dispose() { 
        isDisposed = true;
    }

    public void OnAwake() { }

    public void OnUpdate(float deltaTime) {
        this.updateCount++;

        if (throwException) {
            this.throwException = false;
            throw new ArgumentException();
        }
    }
}
#pragma warning restore 8618
