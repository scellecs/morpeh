namespace Scellecs.Morpeh.Providers;

public class EntityProvider {
    protected Entity cachedEntity;

    protected Entity Entity => this.cachedEntity;

    protected virtual void PreInitialize() {
        
    }
    
    protected virtual void PreDeinitialize() {
        
    }
}