#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif
#if MORPEH_UNITY && MORPEH_BENCHMARK_COLLECTIONS
using System;

namespace Scellecs.Morpeh.Benchmarks.Collections {
    internal static class BenchmarkContainerRunner {
        private const int WARMUP_COUNT = 5;
        private const int MEASURE_COUNT = 10;

        public static void RunComparison<T>(int capacity, BenchmarkContainerType type) where T : IBenchmarkComparisonContainer, new() {
            var container = new T();

            switch (type) {
                case BenchmarkContainerType.BCL:
                    Measure(typeof(T), WARMUP_COUNT, MEASURE_COUNT, () => container.MeasureBCL(), () => container.AllocBCL(capacity));
                    break;

                case BenchmarkContainerType.Morpeh:
                    Measure(typeof(T), WARMUP_COUNT, MEASURE_COUNT, () => container.MeasureMorpeh(), () => container.AllocMorpeh(capacity));
                    break;
            }
        }

        private static void Measure(Type type, int warmupCount, int measurementCount, Action method, Action setup) {
            Unity.PerformanceTesting.Measure
                .Method(method)
                .SampleGroup(type.Name)
                .SetUp(setup)
                .WarmupCount(warmupCount)
                .MeasurementCount(measurementCount)
                .Run();
        }
    }
}
#endif
