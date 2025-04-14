#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

#if MORPEH_UNITY
namespace Scellecs.Morpeh.Experimental {
    internal unsafe struct StructuralChangesBuffer {
        internal void AddEmpty(Entity entity, int typeId) {

        }

        internal void Add(Entity entity, void* data, int typeId) { 
            
        }

        internal void Set(Entity entity, void* data, int typeId) { 
            
        }

        internal void Remove(Entity entity, int typeId) { 
            
        }

        internal bool Has(Entity entity, int typeId) {
            return default;
        }

        internal void* Get(Entity entity, int typeId) {
            return default;
        }

        internal void ApplyChanges() { 
            
        }

        internal void Clear() { 
            
        }
    }
}
#endif
