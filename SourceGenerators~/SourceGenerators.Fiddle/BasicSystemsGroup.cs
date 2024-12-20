namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

[SystemsGroup(inlineUpdateCalls: true)]
public partial class BasicSystemsGroup {
    [Register]
    private BasicDisposableClass _basicDisposableClass1;
    
    [Register(typeof(IDisposable))]
    // [Register(typeof(IntPtr))]
    private BasicDisposableClass _basicDisposableClass2;
    
    private BasicInitializer1 _basicInitializer1;
    
    [Loop(LoopType.Update)]
    private BasicSystem1 _basicSystem1;
    
    [Loop(LoopType.Tick)]
    private BasicGenericSystem<int> _basicGenericSystem;
}