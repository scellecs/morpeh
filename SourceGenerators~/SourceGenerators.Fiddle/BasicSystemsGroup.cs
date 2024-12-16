namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

[SystemsGroup]
public partial class BasicSystemsGroup {
    [Register]
    private BasicDisposableClass _basicDisposableClass;
    
    private BasicInitializer1 _basicInitializer1;
    
    [Loop(LoopType.Update)]
    private BasicSystem1 _basicSystem1;
    
    [Loop(LoopType.Update)]
    private BasicGenericSystem<int> _basicGenericSystem;
}