namespace SourceGenerators.Utils.Pools {
    using System;
    using System.Collections.Generic;
    using NonSemantic;

    public static class IndentSourcePool {
        private const int MAX_POOL_SIZE = 4;
        
        [ThreadStatic]
        private static Stack<IndentSource>? pool;
        
        public static IndentSource Get() {
            pool ??= new Stack<IndentSource>();
            
            if (pool.Count == 0) {
                return new IndentSource();
            }

            return pool.Pop();
        }

        public static void Return(IndentSource indent) {
            pool ??= new Stack<IndentSource>();
            
            if (pool.Count > MAX_POOL_SIZE) {
                return;
            }

            indent.Reset();
            pool.Push(indent);
        }
    }
}