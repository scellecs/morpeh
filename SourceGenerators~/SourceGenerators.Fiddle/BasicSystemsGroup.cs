namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

[SystemsGroup(inlineUpdateCalls: true)]
public partial class BasicSystemsGroup {
    [Register]
    private readonly BasicDisposableClass _basicDisposableClass1;
    
    [Register(typeof(IDisposable))]
    // [Register(typeof(IntPtr))]
    private readonly BasicDisposableClass _basicDisposableClass2;
    
    private readonly BasicInitializer1 _basicInitializer1;
    
    [Loop(LoopType.Update)]
    private readonly BasicSystem1 _basicSystem1;
    
    [Loop(LoopType.Tick)]
    private readonly BasicGenericSystem<int> _basicGenericSystem1;
    
    [Loop(LoopType.TestAnother)]
    private readonly BasicGenericSystem<int> _basicGenericSystem2;
}