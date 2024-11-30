#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif
#if MORPEH_UNITY && MORPEH_BENCHMARK_COLLECTIONS
using System;
namespace Scellecs.Morpeh.Benchmarks.Collections {
    internal static class BenchmarkMeasure {
        public static void Measure(Type type, int warmupCount, int measurementCount, Action method, Action setup) {
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
