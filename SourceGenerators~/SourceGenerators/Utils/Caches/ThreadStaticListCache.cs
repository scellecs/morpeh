namespace SourceGenerators.Utils.Caches {
    using System;
    using System.Collections.Generic;

    public static class ThreadStaticListCache<T> {
        [ThreadStatic]
        private static List<T>? list;

        public static List<T> GetClear() {
            var instance = list ??= new List<T>();
            instance.Clear();
            return instance;
        }
    }
}