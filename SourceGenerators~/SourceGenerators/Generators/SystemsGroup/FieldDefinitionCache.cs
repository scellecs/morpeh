namespace SourceGenerators.Generators.SystemsGroup {
    using System;
    using System.Collections.Generic;

    public static class FieldDefinitionCache {
        [ThreadStatic]
        private static FieldDefinitionCollectionCacheImpl? cache;
        
        [ThreadStatic]
        private static FieldDefinitionPoolImpl? pool;
        
        public static ScopedFieldDefinitionCollection GetScoped() {
            pool  ??= new FieldDefinitionPoolImpl();
            cache ??= new FieldDefinitionCollectionCacheImpl(pool);

            return cache.GetScoped();
        }
    }
    
    public class FieldDefinitionCollectionCacheImpl {
        internal readonly  FieldDefinitionCollection collection = new();
        internal readonly FieldDefinitionPoolImpl    pool;
            
        public FieldDefinitionCollectionCacheImpl(FieldDefinitionPoolImpl pool) => this.pool = pool;
        
        public ScopedFieldDefinitionCollection GetScoped() => new(this);
    }
    
    public readonly struct ScopedFieldDefinitionCollection : IDisposable {
        private readonly FieldDefinitionCollectionCacheImpl cache;
            
        public ScopedFieldDefinitionCollection(FieldDefinitionCollectionCacheImpl cache) => this.cache = cache;
            
        public FieldDefinitionCollection Collection => this.cache.collection;

        public FieldDefinition Emplace() {
            var fieldDefinition = this.cache.pool.Rent();
            this.cache.collection.AddOrdered(fieldDefinition);
            return fieldDefinition;
        }
        
        public void AddToMapping(FieldDefinition fieldDefinition) {
            this.cache.collection.AddToMapping(fieldDefinition);
        }
            
        public void Dispose() {
            for (int i = 0, length = this.cache.collection.ordered.Count; i < length; i++) {
                this.cache.pool.Return(this.cache.collection.ordered[i]);
            }
                    
            this.cache.collection.Clear();
        }
    }
    
    public class FieldDefinitionPoolImpl {
        private readonly Stack<FieldDefinition> pool = new();
        
        public FieldDefinition Rent() => this.pool.Count > 0 ? this.pool.Pop() : new FieldDefinition();
        
        public void Return(FieldDefinition fieldDefinition) {
            fieldDefinition.Reset();
            this.pool.Push(fieldDefinition);
        }
    }
}