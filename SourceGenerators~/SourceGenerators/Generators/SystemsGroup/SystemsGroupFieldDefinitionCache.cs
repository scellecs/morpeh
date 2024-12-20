namespace SourceGenerators.Generators.SystemsGroup {
    using System;
    using System.Collections.Generic;

    public static class SystemsGroupFieldDefinitionCache {
        [ThreadStatic]
        private static SystemsGroupFieldDefinitionCollectionCacheImpl? cache;
        
        [ThreadStatic]
        private static SystemsGroupFieldDefinitionPoolImpl? pool;
        
        public static ScopedSystemsGroupFieldDefinitionCollection GetScoped() {
            pool  ??= new SystemsGroupFieldDefinitionPoolImpl();
            cache ??= new SystemsGroupFieldDefinitionCollectionCacheImpl(pool);

            return cache.GetScoped();
        }
    }
    
    public class SystemsGroupFieldDefinitionCollectionCacheImpl {
        internal readonly  FieldDefinitionCollection collection = new();
        internal readonly SystemsGroupFieldDefinitionPoolImpl    pool;
            
        public SystemsGroupFieldDefinitionCollectionCacheImpl(SystemsGroupFieldDefinitionPoolImpl pool) => this.pool = pool;
        
        public ScopedSystemsGroupFieldDefinitionCollection GetScoped() => new(this);
    }
    
    public readonly struct ScopedSystemsGroupFieldDefinitionCollection : IDisposable {
        private readonly SystemsGroupFieldDefinitionCollectionCacheImpl cache;
            
        public ScopedSystemsGroupFieldDefinitionCollection(SystemsGroupFieldDefinitionCollectionCacheImpl cache) => this.cache = cache;
            
        public FieldDefinitionCollection Collection => this.cache.collection;

        public SystemsGroupFieldDefinition Create() => this.cache.pool.Rent();
        
        public void Add(SystemsGroupFieldDefinition systemsGroupFieldDefinition) {
            this.cache.collection.Add(systemsGroupFieldDefinition);
        }
            
        public void Dispose() {
            for (int i = 0, length = this.cache.collection.ordered.Count; i < length; i++) {
                this.cache.pool.Return(this.cache.collection.ordered[i]);
            }
                    
            this.cache.collection.Clear();
        }
    }
    
    public class SystemsGroupFieldDefinitionPoolImpl {
        private readonly Stack<SystemsGroupFieldDefinition> pool = new();
        
        public SystemsGroupFieldDefinition Rent() => this.pool.Count > 0 ? this.pool.Pop() : new SystemsGroupFieldDefinition();
        
        public void Return(SystemsGroupFieldDefinition systemsGroupFieldDefinition) {
            systemsGroupFieldDefinition.Reset();
            this.pool.Push(systemsGroupFieldDefinition);
        }
    }
}