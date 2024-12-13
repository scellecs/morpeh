namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

[System]
public partial class BasicSystem1 {
    public World World { get; set; }
    
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