namespace Scellecs.Morpeh.Providers;

using UnityEngine;

public class EntityProvider {
    protected Entity     cachedEntity;
    protected GameObject gameObject;

    protected Entity Entity => this.cachedEntity;

    protected virtual void PreInitialize() {
        
    }
    
    protected virtual void PreDeinitialize() {
        
    }
}