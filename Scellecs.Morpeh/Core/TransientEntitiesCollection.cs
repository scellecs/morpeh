namespace Scellecs.Morpeh {
    using System;
    using Unity.IL2CPP.CompilerServices;
    
    // TODO: Merge into World to avoid extra indirection
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal class TransientEntitiesCollection {
        private TransientArchetype[] transients;
        
        public TransientEntitiesCollection(int capacity) {
            this.transients = new TransientArchetype[capacity];
            
            for (var i = 0; i < capacity; i++) {
                this.transients[i] = new TransientArchetype();
            }
        }
        
        public void Rebase(EntityId entityId, Archetype archetype) {
            this.transients[entityId.id].Rebase(archetype);
        }
        
        public void AddComponent(EntityId entityId, TypeInfo typeInfo) {
            this.transients[entityId.id].AddComponent(typeInfo);
        }

        public void RemoveComponent(EntityId entityId, TypeInfo typeInfo) {
            this.transients[entityId.id].RemoveComponent(typeInfo);
        }
        
        public TransientArchetype Get(EntityId entityId) {
            return this.transients[entityId.id];
        }

        public void Resize(int capacity) {
            var oldSize = this.transients.Length;
            
            Array.Resize(ref this.transients, capacity);
            
            for (var i = oldSize; i < capacity; i++) {
                this.transients[i] = new TransientArchetype();
            }
        }
    }
}