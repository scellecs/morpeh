namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

[EcsSystemsGroup(inlineUpdateCalls: true)]
public partial struct StructSystemsGroup {
    [Register]
    private readonly BasicDisposableClass _basicDisposableClass1;
    
    [Register(typeof(IDisposable))]
    // [Register(typeof(IntPtr))]
    private readonly BasicDisposableClass _basicDisposableClass2;
    
    private readonly BasicInitializer1       _basicInitializer1;
    private readonly BasicSystem1            _basicSystem1;
    private readonly BasicGenericSystem<int> _basicGenericSystem1;
    private readonly BasicGenericSystem<int> _basicGenericSystem2;
}