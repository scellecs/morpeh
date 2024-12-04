#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

#if MORPEH_UNITY && MORPEH_BENCHMARK_COLLECTIONS
namespace Scellecs.Morpeh.Benchmarks.Collections {
    internal interface IBenchmarkComparisonContainer {
        public void AllocMorpeh(int capacity);
        public void AllocBCL(int capacity);
        public void MeasureMorpeh();
        public void MeasureBCL();
    }
}
#endif
