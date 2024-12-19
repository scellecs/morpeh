﻿namespace Test.Namespace;

using Scellecs.Morpeh;

[Component]
[StashInitialCapacity(32)]
internal partial struct DisposableComponent : System.IDisposable {
    public int value;
    
    public void Dispose() {
        throw new System.NotImplementedException();
    }
}