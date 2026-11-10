namespace SourceGenerators.Utils.Pools {
    using System.Collections.Concurrent;
    using NonSemantic;

    public static class IndentSourcePool {
        private const int MAX_POOL_SIZE = 32;
        
        private static readonly ConcurrentStack<IndentSource> pool = new();
        
        public static IndentSource Get() => pool.TryPop(out var indent) ? indent : new IndentSource();

        public static void Return(IndentSource indent) {
            if (pool.Count > MAX_POOL_SIZE) {
                return;
            }

            indent.Reset();
            pool.Push(indent);
        }
    }
}