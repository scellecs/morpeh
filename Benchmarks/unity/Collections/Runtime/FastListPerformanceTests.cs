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
    [BenchmarkComparison(typeof(BenchmarkContainerType), "FastList", "List")]
    internal sealed class FastListPerformanceTests {
        //[Test, Performance]
        [Category("Performance")]
        public void IndexerRead([Values(100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<IndexerRead>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void IndexerReadDirect([Values(100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<IndexerReadDirect>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void IndexerWrite([Values(100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<IndexerWrite>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void IndexerWriteDirect([Values(100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<IndexerWriteDirect>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void Add([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<Add>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void AddGrow([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<AddGrow>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void Remove([Values(10_000, 100_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<Remove>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void RemoveAt([Values(10_000, 100_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<RemoveAt>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void RemoveAtFast([Values(10_000, 100_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<RemoveAtFast>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void RemoveRange([Values(10_000, 100_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<RemoveRange>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void RemoveAtSwapBack([Values(10_000, 100_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<RemoveAtSwapBack>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void RemoveAtSwapBackFast([Values(10_000, 100_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<RemoveAtSwapBackFast>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void ForEach([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<ForEach>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void Clear([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<Clear>(count, type);
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

    internal sealed class IndexerRead : IBenchmarkComparisonContainer {
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

    internal sealed class IndexerWrite : IBenchmarkComparisonContainer {
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

    internal sealed class IndexerReadDirect : IBenchmarkComparisonContainer {
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

    internal sealed class IndexerWriteDirect : IBenchmarkComparisonContainer {
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

    internal sealed class Remove : IBenchmarkComparisonContainer {
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

    internal sealed class RemoveAt : IBenchmarkComparisonContainer {
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

    internal sealed class RemoveAtFast : IBenchmarkComparisonContainer {
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

    internal sealed class RemoveRange : IBenchmarkComparisonContainer {
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

    internal sealed class RemoveAtSwapBack : IBenchmarkComparisonContainer {
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

    internal sealed class RemoveAtSwapBackFast : IBenchmarkComparisonContainer {
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

    internal sealed class Add : IBenchmarkComparisonContainer {
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

    internal sealed class AddGrow : IBenchmarkComparisonContainer {
        private FastList<int> fastList;
        private List<int> bclList;
        private int capacity;

        public void AllocBCL(int capacity) {
            this.capacity = capacity;
            this.bclList = new List<int>(4);
        }

        public void AllocMorpeh(int capacity) {
            this.capacity = capacity;
            this.fastList = new FastList<int>(4);
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

    internal sealed class ForEach : IBenchmarkComparisonContainer {
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

    internal sealed class Clear : IBenchmarkComparisonContainer {
        private FastList<int> fastList;
        private List<int> bclList;

        public void AllocBCL(int capacity) {
            this.bclList = InitBCL(capacity, true);
        }

        public void AllocMorpeh(int capacity) {
            this.fastList = InitMorpeh(capacity, true);
        }

        public void MeasureBCL() {
            this.bclList.Clear();
        }

        public void MeasureMorpeh() {
            this.fastList.Clear();
        }
    }
}
#endif
