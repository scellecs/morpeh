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
using static Scellecs.Morpeh.Benchmarks.Collections.IntHashSet.IntHashSetTestsUtility;

namespace Scellecs.Morpeh.Benchmarks.Collections.IntHashSet {
    using IntHashSet = Scellecs.Morpeh.Collections.IntHashSet;

    [BenchmarkComparison(typeof(BenchmarkContainerType), "IntHashSet", "HashSet")]
    internal sealed class IntHashSetPerformanceTests {
        //[Test, Performance]
        [Category("Performance")]
        public void Has([Values(10_000, 100_000, 1_000_000)] int capacity, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<Has>(capacity, type);
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
        public void Remove([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<Remove>(count, type);
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

    internal static class IntHashSetTestsUtility {
        public static HashSet<int> InitBCL(int capacity, bool addValues, out List<int> values) {
            var set = new HashSet<int>(capacity);
            values = new List<int>(capacity);
            
            if (!addValues) {
                return set;
            }
                
            var random = new Random(52);
            int added = 0;
            while (added < capacity) {
                var value = random.Next();
                if (set.Add(value)) {
                    values.Add(value);
                    added++;
                }
            }
            Shuffle(values);
            return set;
        }
        
        public static IntHashSet InitMorpeh(int capacity, bool addValues, out List<int> values) {
            var set = new IntHashSet(capacity);
            values = new List<int>(capacity);
            
            if (!addValues) {
                return set;
            }
                
            var random = new Random(52);
            int added = 0;
            while (added < capacity) {
                var value = random.Next();
                if (set.Add(value)) {
                    values.Add(value);
                    added++;
                }
            }
            Shuffle(values);
            return set;
        }

        public static List<int> CreateRandomUniqueValues(int capacity) {
            var values = new List<int>(capacity);
            var set = new HashSet<int>();
            var random = new Random(69);
            while (values.Count < capacity) {
                var value = random.Next(0, capacity);
                if (set.Add(value)) {
                    values.Add(value);
                }
            }
            return values;
        }

        public static void Shuffle<T>(List<T> list) {
            var random = new Random(69);
            int n = list.Count;
            for (int i = 0; i < n; i++) {
                var itemAt = list[i];
                var randomIndex = random.Next(0, n - 1);
                list[i] = list[randomIndex];
                list[randomIndex] = itemAt;
            }
        }
    }

    internal sealed class Has : IBenchmarkComparisonContainer {
        private HashSet<int> bclSet;
        private IntHashSet morpehSet;
        private List<int> values;

        public void AllocBCL(int capacity) {
            this.bclSet = InitBCL(capacity, true, out this.values);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehSet = InitMorpeh(capacity, true, out this.values);
        }

        public void MeasureBCL() {
            var result = false;
            for (int i = 0; i < values.Count; i++) {
                Volatile.Write(ref result, this.bclSet.Contains(this.values[i]));
            }
        }

        public void MeasureMorpeh() {
            var result = false;
            for (int i = 0; i < values.Count; i++) {
                Volatile.Write(ref result, this.morpehSet.Has(this.values[i]));
            }
        }
    }

    internal sealed class Remove : IBenchmarkComparisonContainer {
        private HashSet<int> bclSet;
        private IntHashSet morpehSet;
        private List<int> values;

        public void AllocBCL(int capacity) {
            this.bclSet = InitBCL(capacity, true, out this.values);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehSet = InitMorpeh(capacity, true, out this.values);
        }

        public void MeasureBCL() {
            foreach (var value in values) {
                this.bclSet.Remove(value);
            }
        }

        public void MeasureMorpeh() {
            foreach (var value in values) {
                this.morpehSet.Remove(value);
            }
        }
    }

    internal sealed class ForEach : IBenchmarkComparisonContainer {
        private HashSet<int> bclSet;
        private IntHashSet morpehSet;

        public void AllocBCL(int capacity) {
            this.bclSet = InitBCL(capacity, true, out _);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehSet = InitMorpeh(capacity, true, out _);
        }

        public void MeasureBCL() {
            foreach (var value in bclSet) {
                var read = value;
                Volatile.Read(ref read);
            }
        }

        public void MeasureMorpeh() {
            foreach (var value in morpehSet) {
                var read = value;
                Volatile.Read(ref read);
            }
        }
    }

    internal sealed class Add : IBenchmarkComparisonContainer {
        private HashSet<int> bclSet;
        private IntHashSet morpehSet;
        private List<int> values;

        public void AllocBCL(int capacity) {
            this.bclSet = InitBCL(capacity, false, out _);
            this.values = CreateRandomUniqueValues(capacity);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehSet = InitMorpeh(capacity, false, out _);
            this.values = CreateRandomUniqueValues(capacity);
        }

        public void MeasureBCL() {
            foreach (var value in values) {
                this.bclSet.Add(value);
            }
        }

        public void MeasureMorpeh() {
            foreach (var value in values) {
                this.morpehSet.Add(value);
            }
        }
    }

    internal sealed class AddGrow : IBenchmarkComparisonContainer {
        private HashSet<int> bclSet;
        private IntHashSet morpehSet;
        private List<int> values;

        public void AllocBCL(int capacity) {
            this.bclSet = new HashSet<int>(4);
            this.values = CreateRandomUniqueValues(capacity);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehSet = new IntHashSet(4);
            this.values = CreateRandomUniqueValues(capacity);
        }

        public void MeasureBCL() {
            foreach (var value in values) {
                this.bclSet.Add(value);
            }
        }

        public void MeasureMorpeh() {
            foreach (var value in values) {
                this.morpehSet.Add(value);
            }
        }
    }

    internal sealed class Clear : IBenchmarkComparisonContainer {
        private HashSet<int> bclSet;
        private IntHashSet morpehSet;

        public void AllocBCL(int capacity) {
            this.bclSet = InitBCL(capacity, true, out _);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehSet = InitMorpeh(capacity, true, out _);
        }

        public void MeasureBCL() {
            this.bclSet.Clear();
        }

        public void MeasureMorpeh() {
            this.morpehSet.Clear();
        }
    }
}
#endif