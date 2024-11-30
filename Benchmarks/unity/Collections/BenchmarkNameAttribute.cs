#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif
#if MORPEH_UNITY && MORPEH_BENCHMARK_COLLECTIONS
using System;
namespace Scellecs.Morpeh.Benchmarks.Collections {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BenchmarkComparisonAttribute : Attribute {
        public string BCL { get; }
        public string Morpeh { get; }

        public BenchmarkComparisonAttribute(string bcl, string morpeh) {
            BCL = bcl;
            Morpeh = morpeh;
        }
    }
}
#endif
