namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

public partial struct BasicFeature {
    [EcsSystemsGroup(inlineUpdateCalls: true)]
    public partial class Update {
        [Register]
        private readonly BasicDisposableClass _basicDisposableClass1;
    
        [Register(typeof(IDisposable))]
        // [Register(typeof(IntPtr))]
        private readonly BasicDisposableClass _basicDisposableClass2;
    
        private readonly BasicInitializer1 _basicInitializer1;
    
        [EcsLoop(LoopType.Update)]
        private readonly BasicSystem1 _basicSystem1;
    
        [EcsLoop(LoopType.Tick)]
        private readonly BasicGenericSystem<int> _basicGenericSystem1;
    
        [EcsLoop(LoopType.TestAnother)]
        private readonly BasicGenericSystem<int> _basicGenericSystem2;
    }

    public partial class InnerComponentClassForTesting {
        [EcsComponent]
        public partial struct FeatureComponent { }

        public partial class InnerSystemClassForTesting {
            [EcsSystem]
            internal partial class BasicSystem1 {
                public World World { get; }
    
                public bool IsEnabled() => throw new NotImplementedException();
    
                public void OnAwake() {
                    throw new NotImplementedException();
                }
    
                public void OnUpdate(float deltaTime) {
                    throw new NotImplementedException();
                }
    
                public void Dispose() {
                    throw new NotImplementedException();
                }
            }
        }
    }
}