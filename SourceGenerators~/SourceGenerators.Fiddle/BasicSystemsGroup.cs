﻿namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

[SystemsGroup]
[SystemGroupInlineUpdateMethods]
public partial class BasicSystemsGroup {
    [Register]
    private BasicDisposableClass _basicDisposableClass1;
    
    [Register(typeof(IDisposable))]
    private BasicDisposableClass _basicDisposableClass2;
    
    private BasicInitializer1 _basicInitializer1;
    
    [Loop(LoopType.Update)]
    private BasicSystem1 _basicSystem1;
    
    [Loop(LoopType.Update)]
    private BasicGenericSystem<int> _basicGenericSystem;
}