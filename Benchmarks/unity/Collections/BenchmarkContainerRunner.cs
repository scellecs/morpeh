#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif
#if MORPEH_UNITY && MORPEH_BENCHMARK_COLLECTIONS
namespace Scellecs.Morpeh.Benchmarks.Collections {
    internal static class BenchmarkContainerRunner<T> where T : IBenchmarkContainer, new() {
        public static void Run(int capacity, BenchmarkContainerType type) {
            var container = new T();

            switch (type) {
                case BenchmarkContainerType.BCL:
                    BenchmarkMeasure.Measure(typeof(T), 5, 10, () => container.MeasureBCL(), () => container.AllocBCL(capacity));
                    break;

                case BenchmarkContainerType.Morpeh:
                    BenchmarkMeasure.Measure(typeof(T), 5, 10, () => container.MeasureMorpeh(), () => container.AllocMorpeh(capacity));
                    break;
            }
        }
    }
}
#endif
