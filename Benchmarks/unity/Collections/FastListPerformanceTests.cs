#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif
#if MORPEH_UNITY && MORPEH_BENCHMARK_COLLECTIONS
using NUnit.Framework;
using Scellecs.Morpeh.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.PerformanceTesting;
using static Scellecs.Morpeh.Benchmarks.Collections.FastList.FastListBenchmarkUtility;
namespace Scellecs.Morpeh.Benchmarks.Collections.FastList {
    [Benchmark("List", "FastList")]
    internal sealed class FastListBenchmark {
        [Test, Performance]
        [Category("Performance")]
        public void IndexerRead([Values(100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<IndexerRead>.Run(count, type);
        }

        [Test, Performance]
        [Category("Performance")]
        public void IndexerReadDirect([Values(100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<IndexerReadDirect>.Run(count, type);
        }

        [Test, Performance]
        [Category("Performance")]
        public void IndexerWrite([Values(100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<IndexerWrite>.Run(count, type);
        }

        [Test, Performance]
        [Category("Performance")]
        public void IndexerWriteDirect([Values(100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<IndexerWriteDirect>.Run(count, type);
        }

        [Test, Performance]
        [Category("Performance")]
        public void Remove([Values(10_000, 100_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<Remove>.Run(count, type);
        }

        [Test, Performance]
        [Category("Performance")]
        public void RemoveAt([Values(10_000, 100_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<RemoveAt>.Run(count, type);
        }

        [Test, Performance]
        [Category("Performance")]
        public void RemoveAtFast([Values(10_000, 100_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<RemoveAtFast>.Run(count, type);
        }

        [Test, Performance]
        [Category("Performance")]
        public void RemoveRange([Values(10_000, 100_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<RemoveRange>.Run(count, type);
        }

        [Test, Performance]
        [Category("Performance")]
        public void RemoveAtSwapBack([Values(10_000, 100_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<RemoveAtSwapBack>.Run(count, type);
        }

        [Test, Performance]
        [Category("Performance")]
        public void RemoveAtSwapBackFast([Values(10_000, 100_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<RemoveAtSwapBackFast>.Run(count, type);
        }

        [Test, Performance]
        public void ForEach([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<ForEach>.Run(count, type);
        }
    }

    internal static class FastListBenchmarkUtility {
        public static List<int> InitBCL(int capacity, bool addValues) {
            var list = new List<int>(capacity);
            if (addValues) {
                for (int i = 0; i < capacity; i++) {
                    list.Add(i);
                }
            }

            return list;
        }

        public static FastList<int> InitMorpeh(int capacity, bool addValues) {
            var list = new FastList<int>(capacity);
            if (addValues) {
                for (int i = 0; i < capacity; i++) {
                    list.Add(i);
                }
            }

            return list;
        }

        public static List<int> InitRandomValues(int capacity) {
            var values = new List<int>(capacity);
            var random = new Random(69);
            for (int i = 0; i < capacity; i++) {
                var value = random.Next(0, capacity);
                values.Add(value);
            }

            return values;
        }
    }

    internal sealed class IndexerRead : IBenchmarkContainer {
        private FastList<int> fastList;
        private List<int> bclList;
        private List<int> values;

        public void AllocBCL(int capacity) {
            this.bclList = InitBCL(capacity, true);
            this.values = InitRandomValues(capacity);
        }

        public void AllocMorpeh(int capacity) {
            this.fastList = InitMorpeh(capacity, true);
            this.values = InitRandomValues(capacity);
        }

        public void MeasureBCL() {
            var count = this.values.Count;
            var value = 0;

            for (int i = 0; i < count; i++) {
                Volatile.Write(ref value, this.bclList[this.values[i]]);
            }
        }

        public void MeasureMorpeh() {
            var count = this.values.Count;
            var value = 0;

            for (int i = 0; i < count; i++) {
                Volatile.Write(ref value, this.fastList[this.values[i]]);
            }
        }
    }

    internal sealed class IndexerWrite : IBenchmarkContainer {
        private FastList<int> fastList;
        private List<int> bclList;
        private List<int> values;

        public void AllocBCL(int capacity) {
            this.bclList = InitBCL(capacity, true);
            this.values = InitRandomValues(capacity);
        }

        public void AllocMorpeh(int capacity) {
            this.fastList = InitMorpeh(capacity, true);
            this.values = InitRandomValues(capacity);
        }

        public void MeasureBCL() {
            var count = this.values.Count;

            for (int i = 0; i < count; i++) {
                this.bclList[this.values[i]] = i;
            }
        }

        public void MeasureMorpeh() {
            var count = this.values.Count;

            for (int i = 0; i < count; i++) {
                this.fastList[this.values[i]] = i;
            }
        }
    }

    internal sealed class IndexerReadDirect : IBenchmarkContainer {
        private FastList<int> fastList;
        private List<int> bclList;
        private List<int> values;

        public void AllocBCL(int capacity) {
            this.bclList = InitBCL(capacity, true);
            this.values = InitRandomValues(capacity);
        }

        public void AllocMorpeh(int capacity) {
            this.fastList = InitMorpeh(capacity, true);
            this.values = InitRandomValues(capacity);
        }

        public void MeasureBCL() {
            var count = this.values.Count;
            var value = 0;

            for (int i = 0; i < count; i++) {
                Volatile.Write(ref value, this.bclList[this.values[i]]);
            }
        }

        public void MeasureMorpeh() {
            var count = this.values.Count;
            var value = 0;

            for (int i = 0; i < count; i++) {
                Volatile.Write(ref value, this.fastList.data[this.values[i]]);
            }
        }
    }

    internal sealed class IndexerWriteDirect : IBenchmarkContainer {
        private FastList<int> fastList;
        private List<int> bclList;
        private List<int> values;

        public void AllocBCL(int capacity) {
            this.bclList = InitBCL(capacity, true);
            this.values = InitRandomValues(capacity);
        }

        public void AllocMorpeh(int capacity) {
            this.fastList = InitMorpeh(capacity, true);
            this.values = InitRandomValues(capacity);
        }

        public void MeasureBCL() {
            var count = this.values.Count;

            for (int i = 0; i < count; i++) {
                this.bclList[this.values[i]] = i;
            }
        }

        public void MeasureMorpeh() {
            var count = this.values.Count;

            for (int i = 0; i < count; i++) {
                this.fastList.data[this.values[i]] = i;
            }
        }
    }

    internal sealed class Remove : IBenchmarkContainer {
        private FastList<int> fastList;
        private List<int> bclList;
        private List<int> values;

        public void AllocBCL(int capacity) {
            this.bclList = InitBCL(capacity, false);
            this.values = InitRandomValues(capacity);

            foreach (var value in values) {
                this.bclList.Add(value);
            }
        }

        public void AllocMorpeh(int capacity) {
            this.fastList = InitMorpeh(capacity, false);
            this.values = InitRandomValues(capacity);

            foreach (var value in values) {
                this.fastList.Add(value);
            }
        }

        public void MeasureBCL() {
            var count = values.Count;

            for (int i = 0; i < count; i++) {
                this.bclList.Remove(this.values[i]);
            }
        }

        public void MeasureMorpeh() {
            var count = values.Count;

            for (int i = 0; i < count; i++) {
                this.fastList.Remove(this.values[i]);
            }
        }
    }

    internal sealed class RemoveAt : IBenchmarkContainer {
        private FastList<int> fastList;
        private List<int> bclList;
        private List<int> values;

        private void FixValues(List<int> values) {
            var max = values.Count;
            while (--max >= 0) {
                var reverseIndex = values.Count - 1 - max;
                var value = values[reverseIndex];
                if (value > max) {
                    values[reverseIndex] = max;
                }
            }
        }

        public void AllocBCL(int capacity) {
            this.bclList = InitBCL(capacity, true);
            this.values = InitRandomValues(capacity);
            this.FixValues(this.values);
        }

        public void AllocMorpeh(int capacity) {
            this.fastList = InitMorpeh(capacity, true);
            this.values = InitRandomValues(capacity);
            this.FixValues(this.values);
        }

        public void MeasureBCL() {
            var count = values.Count;
            for (int i = 0; i < count; i++) {
                this.bclList.RemoveAt(values[i]);
            }
        }

        public void MeasureMorpeh() {
            var count = values.Count;
            for (int i = 0; i < count; i++) {
                this.fastList.RemoveAt(values[i]);
            }
        }
    }

    internal sealed class RemoveAtFast : IBenchmarkContainer {
        private FastList<int> fastList;
        private List<int> bclList;
        private List<int> values;

        private void FixValues(List<int> values) {
            var max = values.Count;
            while (--max >= 0) {
                var reverseIndex = values.Count - 1 - max;
                var value = values[reverseIndex];
                if (value > max) {
                    values[reverseIndex] = max;
                }
            }
        }

        public void AllocBCL(int capacity) {
            this.bclList = InitBCL(capacity, true);
            this.values = InitRandomValues(capacity);
            this.FixValues(this.values);
        }

        public void AllocMorpeh(int capacity) {
            this.fastList = InitMorpeh(capacity, true);
            this.values = InitRandomValues(capacity);
            this.FixValues(this.values);
        }

        public void MeasureBCL() {
            var count = values.Count;
            for (int i = 0; i < count; i++) {
                this.bclList.RemoveAt(values[i]);
            }
        }

        public void MeasureMorpeh() {
            var count = values.Count;
            for (int i = 0; i < count; i++) {
                this.fastList.RemoveAtFast(values[i]);
            }
        }
    }

    internal sealed class RemoveRange : IBenchmarkContainer {
        private FastList<int> fastList;
        private List<int> bclList;
        private List<int> values;

        public void AllocBCL(int capacity) {
            this.bclList = InitBCL(capacity, false);
            this.values = InitRandomValues(capacity);

            foreach (var value in values) {
                this.bclList.Add(value);
            }
        }

        public void AllocMorpeh(int capacity) {
            this.fastList = InitMorpeh(capacity, false);
            this.values = InitRandomValues(capacity);

            foreach (var value in values) {
                this.fastList.Add(value);
            }
        }

        public void MeasureBCL() {
            var count = values.Count;
            var segment = count / 10;

            for (int i = 0; i < 10; i++) {
                this.bclList.RemoveRange(0, segment);
            }
        }

        public void MeasureMorpeh() {
            var count = values.Count;
            var segment = count / 10;

            for (int i = 0; i < 10; i++) {
                this.fastList.RemoveRange(0, segment);
            }
        }
    }

    internal sealed class RemoveAtSwapBack : IBenchmarkContainer {
        private FastList<int> fastList;
        private List<int> bclList;
        private List<int> values;

        private void FixValues(List<int> values) {
            var max = values.Count;
            while (--max >= 0) {
                var reverseIndex = values.Count - 1 - max;
                var value = values[reverseIndex];
                if (value > max) {
                    values[reverseIndex] = max;
                }
            }
        }

        public void AllocBCL(int capacity) {
            this.bclList = InitBCL(capacity, true);
            this.values = InitRandomValues(capacity);
            this.FixValues(this.values);
        }

        public void AllocMorpeh(int capacity) {
            this.fastList = InitMorpeh(capacity, true);
            this.values = InitRandomValues(capacity);
            this.FixValues(this.values);
        }

        public void MeasureBCL() {
            var count = values.Count;
            for (int i = 0; i < count; i++) {
                var index = values[i];
                var lastIndex = this.bclList.Count - 1;
                this.bclList[index] = this.bclList[lastIndex];
                this.bclList.RemoveAt(lastIndex);
            }
        }

        public void MeasureMorpeh() {
            var count = values.Count;
            for (int i = 0; i < count; i++) {
                this.fastList.RemoveAtSwapBack(values[i]);
            }
        }
    }

    internal sealed class RemoveAtSwapBackFast : IBenchmarkContainer {
        private FastList<int> fastList;
        private List<int> bclList;
        private List<int> values;

        private void FixValues(List<int> values) {
            var max = values.Count;
            while (--max >= 0) {
                var reverseIndex = values.Count - 1 - max;
                var value = values[reverseIndex];
                if (value > max) {
                    values[reverseIndex] = max;
                }
            }
        }

        public void AllocBCL(int capacity) {
            this.bclList = InitBCL(capacity, true);
            this.values = InitRandomValues(capacity);
            this.FixValues(this.values);
        }

        public void AllocMorpeh(int capacity) {
            this.fastList = InitMorpeh(capacity, true);
            this.values = InitRandomValues(capacity);
            this.FixValues(this.values);
        }

        public void MeasureBCL() {
            var count = values.Count;
            for (int i = 0; i < count; i++) {
                var index = values[i];
                var lastIndex = this.bclList.Count - 1;
                this.bclList[index] = this.bclList[lastIndex];
                this.bclList.RemoveAt(lastIndex);
            }
        }

        public void MeasureMorpeh() {
            var count = values.Count;
            for (int i = 0; i < count; i++) {
                this.fastList.RemoveAtSwapBackFast(values[i]);
            }
        }
    }

    internal sealed class Add : IBenchmarkContainer {
        private FastList<int> fastList;
        private List<int> bclList;
        private int capacity;

        public void AllocBCL(int capacity) {
            this.bclList = InitBCL(capacity, false);
            this.capacity = capacity;
        }

        public void AllocMorpeh(int capacity) {
            this.fastList = InitMorpeh(capacity, false);
            this.capacity = capacity;
        }

        public void MeasureBCL() {
            for (int i = 0; i < capacity; i++) {
                this.bclList.Add(i);
            }
        }

        public void MeasureMorpeh() {
            for (int i = 0; i < capacity; i++) {
                this.fastList.Add(i);
            }
        }
    }

    internal sealed class ForEach : IBenchmarkContainer {
        private FastList<int> fastList;
        private List<int> bclList;

        public void AllocBCL(int capacity) {
            this.bclList = InitBCL(capacity, true);
        }

        public void AllocMorpeh(int capacity) {
            this.fastList = InitMorpeh(capacity, true);
        }

        public void MeasureBCL() {
            int value = 0;
            foreach (var element in bclList) {
                Volatile.Write(ref value, element);
            }
        }

        public void MeasureMorpeh() {
            int value = 0;
            foreach (var element in fastList) {
                Volatile.Write(ref value, element);
            }
        }
    }
}
#endif
