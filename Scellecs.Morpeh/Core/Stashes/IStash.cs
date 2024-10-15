namespace Scellecs.Morpeh {
    using System;
    
    public interface IStash : IDisposable { 
        public Type Type { get; }
        public int Length { get; }
        
        public void Set(Entity entity);
        public bool Remove(Entity entity);
        public void RemoveAll();
        public void Migrate(Entity from, Entity to, bool overwrite = true);
        public bool Has(Entity entity);
        internal void Clean(Entity entity);
    }
}