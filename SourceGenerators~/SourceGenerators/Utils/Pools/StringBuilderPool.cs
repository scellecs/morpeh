namespace SourceGenerators.Utils.Pools {
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class StringBuilderPool {
        private const int MAX_POOL_SIZE = 16;
        
        [ThreadStatic]
        private static Stack<StringBuilder>? pool;
        
        public static StringBuilder Get() {
            pool ??= new Stack<StringBuilder>();
            
            if (pool.Count == 0) {
                return new StringBuilder();
            }

            return pool.Pop();
        }

        public static void Return(StringBuilder sb) {
            pool ??= new Stack<StringBuilder>();
            
            if (pool.Count > MAX_POOL_SIZE) {
                return;
            }
            
            sb.Clear();
            pool.Push(sb);
        }
        
        public static ScopedStringBuilder GetScoped() => new(Get());

        public readonly struct ScopedStringBuilder : IDisposable {
            private readonly StringBuilder sb;
            
            public StringBuilder StringBuilder => this.sb;

            public ScopedStringBuilder(StringBuilder sb) => this.sb = sb;

            public void Dispose() {
                Return(this.sb);
            }
        }
    }
}