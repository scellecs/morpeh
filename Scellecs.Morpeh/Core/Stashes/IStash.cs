namespace Scellecs.Morpeh {
    using System;
    
    public interface IStash : IDisposable { 
        public Type Type { get; }
        public int Length { get; }

        public bool IsEmpty();
        public bool IsNotEmpty();
        
        public void Set(Entity entity);
        public bool Remove(Entity entity);
        public void RemoveAll();
        public void Migrate(Entity from, Entity to, bool overwrite = true);
        public bool Has(Entity entity);
        internal void Clean(Entity entity);
        
        public IComponent GetBoxed(Entity entity);
        public IComponent GetBoxed(Entity entity, out bool exists);
        public void       SetBoxed(Entity entity, IComponent value);
    }
}