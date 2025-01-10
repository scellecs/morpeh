namespace SourceGenerators.Utils.Pools {
    using System.Collections.Concurrent;
    using System.Text;

    public static class StringBuilderPool {
        private const int MAX_POOL_SIZE = 32;

        private static readonly ConcurrentStack<StringBuilder> pool = new();

        public static StringBuilder Get() => pool.TryPop(out var sb) ? sb : new StringBuilder();

        public static void Return(StringBuilder sb) {
            if (pool.Count >= MAX_POOL_SIZE) {
                return;
            }

            sb.Clear();
            pool.Push(sb);
        }

        public static string ToStringAndReturn(this StringBuilder sb) {
            var result = sb.ToString();
            Return(sb);
            return result;
        }
    }
}