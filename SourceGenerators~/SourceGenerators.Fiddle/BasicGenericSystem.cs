namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

[System]
public partial class BasicGenericSystem<T> where T : struct {
    public World World { get; set; }

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