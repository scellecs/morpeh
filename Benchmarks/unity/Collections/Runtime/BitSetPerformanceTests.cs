#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

#if MORPEH_UNITY && MORPEH_BENCHMARK_COLLECTIONS
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.PerformanceTesting;
using static Scellecs.Morpeh.Benchmarks.Collections.BitSet.BitSetTestsUtility;

namespace Scellecs.Morpeh.Benchmarks.Collections.BitSet {
    using BitSet = Scellecs.Morpeh.Collections.BitSet;

    [BenchmarkComparison(typeof(BenchmarkContainerType), "BitSet", "BitArray")]
    internal sealed class BitSetPerformanceTests {
        //[Test, Performance]
        [Category("Performance")]
        public void IsSet([Values(10_000, 100_000, 1_000_000)] int capacity, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<IsSet>(capacity, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void Set([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<Set>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void SetGrow([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<SetGrow>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void Unset([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<Unset>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void Clear([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<Clear>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void ClearManually([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<ClearManually>(count, type);
        }
    }

    internal static class BitSetTestsUtility {
        public static BitArray InitBCL(int capacity, bool setValues, out List<int> indices) {
            var array = new BitArray(capacity);
            indices = new List<int>(capacity);
            
            if (!setValues) {
                return array;
            }
                
            var random = new Random(52);
            int added = 0;
            while (added < capacity) {
                var index = random.Next(0, capacity);
                if (!array.Get(index)) {
                    array.Set(index, true);
                    indices.Add(index);
                    added++;
                }
            }
            Shuffle(indices);
            return array;
        }
        
        public static BitSet InitMorpeh(int capacity, bool setValues, out List<int> indices) {
            var set = new BitSet(capacity);
            indices = new List<int>(capacity);
            
            if (!setValues) {
                return set;
            }
                
            var random = new Random(52);
            int added = 0;
            while (added < capacity) {
                var index = random.Next(0, capacity);
                if (set.Set(index)) {
                    indices.Add(index);
                    added++;
                }
            }
            Shuffle(indices);
            return set;
        }

        public static List<int> CreateRandomIndices(int capacity) {
            var indices = new List<int>(capacity);
            var used = new BitArray(capacity);
            var random = new Random(69);
            while (indices.Count < capacity) {
                var index = random.Next(0, capacity);
                if (!used.Get(index)) {
                    used.Set(index, true);
                    indices.Add(index);
                }
            }
            return indices;
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

    internal sealed class IsSet : IBenchmarkComparisonContainer {
        private BitArray bclSet;
        private BitSet morpehSet;
        private List<int> indices;

        public void AllocBCL(int capacity) {
            this.bclSet = InitBCL(capacity, true, out this.indices);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehSet = InitMorpeh(capacity, true, out this.indices);
        }

        public void MeasureBCL() {
            var result = false;
            for (int i = 0; i < indices.Count; i++) {
                Volatile.Write(ref result, this.bclSet.Get(this.indices[i]));
            }
        }

        public void MeasureMorpeh() {
            var result = false;
            for (int i = 0; i < indices.Count; i++) {
                Volatile.Write(ref result, this.morpehSet.IsSet(this.indices[i]));
            }
        }
    }

    internal sealed class Unset : IBenchmarkComparisonContainer {
        private BitArray bclSet;
        private BitSet morpehSet;
        private List<int> indices;

        public void AllocBCL(int capacity) {
            this.bclSet = InitBCL(capacity, true, out this.indices);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehSet = InitMorpeh(capacity, true, out this.indices);
        }

        public void MeasureBCL() {
            foreach (var index in indices) {
                this.bclSet.Set(index, false);
            }
        }

        public void MeasureMorpeh() {
            foreach (var index in indices) {
                this.morpehSet.Unset(index);
            }
        }
    }

    internal sealed class Set : IBenchmarkComparisonContainer {
        private BitArray bclSet;
        private BitSet morpehSet;
        private List<int> indices;

        public void AllocBCL(int capacity) {
            this.bclSet = InitBCL(capacity, false, out _);
            this.indices = CreateRandomIndices(capacity);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehSet = InitMorpeh(capacity, false, out _);
            this.indices = CreateRandomIndices(capacity);
        }

        public void MeasureBCL() {
            foreach (var index in indices) {
                this.bclSet.Set(index, true);
            }
        }

        public void MeasureMorpeh() {
            foreach (var index in indices) {
                this.morpehSet.Set(index);
            }
        }
    }

    internal sealed class SetGrow : IBenchmarkComparisonContainer {
        private BitArray bclSet;
        private BitSet morpehSet;
        private List<int> indices;

        public void AllocBCL(int capacity) {
            this.bclSet = new BitArray(64);
            this.indices = CreateRandomIndices(capacity);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehSet = new BitSet(64);
            this.indices = CreateRandomIndices(capacity);
        }

        public void MeasureBCL() {
            foreach (var index in indices) {
                if (index >= this.bclSet.Length) {
                    var newArray = new BitArray(index + 64);
                    for (int i = 0; i < this.bclSet.Length; i++) {
                        newArray[i] = this.bclSet[i];
                    }
                    this.bclSet = newArray;
                }
                this.bclSet.Set(index, true);
            }
        }

        public void MeasureMorpeh() {
            foreach (var index in indices) {
                this.morpehSet.Set(index);
            }
        }
    }

    internal sealed class Clear : IBenchmarkComparisonContainer {
        private BitArray bclSet;
        private BitSet morpehSet;

        public void AllocBCL(int capacity) {
            this.bclSet = InitBCL(capacity, true, out _);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehSet = InitMorpeh(capacity, true, out _);
        }

        public void MeasureBCL() {
            this.bclSet.SetAll(false);
        }

        public void MeasureMorpeh() {
            this.morpehSet.Clear();
        }
    }

    internal sealed class ClearManually : IBenchmarkComparisonContainer {
        private BitArray bclSet;
        private BitSet morpehSet;
        private List<int> indices;

        public void AllocBCL(int capacity) {
            this.bclSet = InitBCL(capacity, true, out this.indices);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehSet = InitMorpeh(capacity, true, out this.indices);
        }

        public void MeasureBCL() {
            foreach (var index in indices) {
                this.bclSet.Set(index, false);
            }
        }

        public void MeasureMorpeh() {
            foreach (var index in indices) {
                this.morpehSet.Unset(index);
            }
        }
    }
}
#endif