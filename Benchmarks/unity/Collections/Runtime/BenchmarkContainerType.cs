#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

#if MORPEH_UNITY && MORPEH_BENCHMARK_COLLECTIONS
namespace Scellecs.Morpeh.Benchmarks.Collections {
    internal enum BenchmarkContainerType : int {
        Morpeh,
        BCL,
    }
}
#endif
