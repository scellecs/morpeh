namespace SourceGenerators.Fiddle;

using Scellecs.Morpeh;

[System(skipCommit: true, alwaysEnabled: true)]
public partial class BasicSystemSkipCommit {
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