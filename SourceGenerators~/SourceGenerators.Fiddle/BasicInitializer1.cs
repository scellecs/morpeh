﻿namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;
using Test.Namespace;

[Initializer]
[Require(typeof(TagComponent))]
[Require(typeof(GenericComponent<int>))]
[Require(typeof(GlobalNamespaceComponent))]
[Require(typeof(DisposableComponent))]
public partial class BasicInitializer1 {
    [Injectable]
    private BasicDisposableClass _basicDisposableClass1;
    
    [Injectable]
    private IDisposable _basicDisposableClass2;
    
    public void OnAwake() {
        throw new NotImplementedException();
    }
    
    public void Dispose() {
        throw new NotImplementedException();
    }
}