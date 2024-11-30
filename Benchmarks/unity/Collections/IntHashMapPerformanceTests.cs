﻿#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif
#if MORPEH_UNITY && MORPEH_BENCHMARK_COLLECTIONS
using NUnit.Framework;
using Scellecs.Morpeh.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.PerformanceTesting;
using static Scellecs.Morpeh.Benchmarks.Collections.IntHashMap.IntHashMapTestsUtility;
namespace Scellecs.Morpeh.Benchmarks.Collections.IntHashMap {
    [BenchmarkComparison("Dictionary", "IntHashMap")]
    internal sealed class IntHashMapPerformanceTests {
        [Test, Performance]
        [Category("Performance")]
        public void IndexerRead([Values(10_000, 100_000, 1_000_000)] int capacity, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<IndexerRead>.Run(capacity, type);
        }

        [Test, Performance]
        public void Add([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<Add>.Run(count, type);
        }

        [Test, Performance]
        public void AddGrow([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<AddGrow>.Run(count, type);
        }

        [Test, Performance]
        [Category("Performance")]
        public void TryGetValue([Values(10_000, 100_000, 1_000_000)] int capacity, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<TryGetValue>.Run(capacity, type);
        }

        [Test, Performance]
        [Category("Performance")]
        public void Remove([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<Remove>.Run(count, type);
        }

        [Test, Performance]
        public void ForEach([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner<ForEach>.Run(count, type);
        }
    }

    internal static class IntHashMapTestsUtility {
        public static List<int> CreateRandomUniqueKeys(int capacity) {
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
                var keyAt = list[i];
                var randomIndex = random.Next(0, n - 1);
                list[i] = list[randomIndex];
                list[randomIndex] = keyAt;
            }
        }
    }

    internal sealed class IndexerRead : IBenchmarkComparisonContainer {
        private Dictionary<int, int> bclMap;
        private IntHashMap<int> morpehMap;
        private List<int> keys;

        public void AllocBCL(int capacity) {
            this.bclMap = new Dictionary<int, int>(capacity);
            this.keys = new List<int>(capacity);
            var random = new Random(52);
            int added = 0;
            while (added < capacity) {
                var key = random.Next();
                if (this.bclMap.TryAdd(key, added)) {
                    this.keys.Add(key);
                    added++;
                }
            }
            Shuffle(this.keys);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = new IntHashMap<int>(capacity);
            this.keys = new List<int>(capacity);
            var random = new Random(52);
            int added = 0;
            while (added < capacity) {
                var key = random.Next();
                if (this.morpehMap.Add(key, added, out _)) {
                    this.keys.Add(key);
                    added++;
                }
            }
            Shuffle(this.keys);
        }

        public void MeasureBCL() {
            var value = 0;
            for (int i = 0; i < keys.Count; i++) {
                Volatile.Write(ref value, this.bclMap[this.keys[i]]);
            }
        }

        public void MeasureMorpeh() {
            var value = 0;
            for (int i = 0; i < keys.Count; i++) {
                Volatile.Write(ref value, this.morpehMap.GetValueByKey(this.keys[i]));
            }
        }
    }

    internal sealed class TryGetValue : IBenchmarkComparisonContainer {
        private Dictionary<int, int> bclMap;
        private IntHashMap<int> morpehMap;
        private List<int> keys;

        public void AllocBCL(int capacity) {
            this.bclMap = new Dictionary<int, int>(capacity);
            this.keys = new List<int>(capacity);
            var random = new Random(52);
            int added = 0;
            while (added < capacity) {
                var key = random.Next();
                if (this.bclMap.TryAdd(key, added)) {
                    this.keys.Add(key);
                    added++;
                }
            }
            Shuffle(this.keys);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = new IntHashMap<int>(capacity);
            this.keys = new List<int>(capacity);
            var random = new Random(52);
            int added = 0;
            while (added < capacity) {
                var key = random.Next();
                if (this.morpehMap.Add(key, added, out _)) {
                    this.keys.Add(key);
                    added++;
                }
            }
            Shuffle(this.keys);
        }

        public void MeasureBCL() {
            for (int i = 0; i < keys.Count; i++) {
                this.bclMap.TryGetValue(this.keys[i], out var value);
                Volatile.Read(ref value);
            }
        }

        public void MeasureMorpeh() {
            for (int i = 0; i < keys.Count; i++)
            {
                this.morpehMap.TryGetValue(this.keys[i], out var value);
                Volatile.Read(ref value);
            }
        }
    }

    internal sealed class Remove : IBenchmarkComparisonContainer {
        private Dictionary<int, int> bclMap;
        private IntHashMap<int> morpehMap;
        private List<int> keys;

        public void AllocBCL(int capacity) {
            this.bclMap = new Dictionary<int, int>(capacity);
            this.keys = new List<int>(capacity);
            var random = new Random(52);
            int added = 0;
            while (added < capacity) {
                var key = random.Next();
                if (this.bclMap.TryAdd(key, added)) {
                    this.keys.Add(key);
                    added++;
                }
            }
            Shuffle(this.keys);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = new IntHashMap<int>(capacity);
            this.keys = new List<int>(capacity);
            var random = new Random(52);
            int added = 0;
            while (added < capacity) {
                var key = random.Next();
                if (this.morpehMap.Add(key, added, out _)) {
                    this.keys.Add(key);
                    added++;
                }
            }
            Shuffle(this.keys);
        }

        public void MeasureBCL() {
            foreach (var key in keys) {
                this.bclMap.Remove(key);
            }
        }

        public void MeasureMorpeh() {
            foreach (var key in keys) {
                this.morpehMap.Remove(key, out _);
            }
        }
    }

    internal sealed class ForEach : IBenchmarkComparisonContainer {
        private Dictionary<int, int> bclMap;
        private IntHashMap<int> morpehMap;

        public void AllocBCL(int capacity) {
            this.bclMap = new Dictionary<int, int>(capacity);
            for (int i = 0; i < capacity; i++) {
                this.bclMap.Add(i, i);
            }
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = new IntHashMap<int>(capacity);
            for (int i = 0; i < capacity; i++) {
                this.morpehMap.Add(i, i, out _);
            }
        }

        public void MeasureBCL() {
            foreach (var pair in bclMap) {
                var value = pair.Value;
                Volatile.Read(ref value);
            }
        }

        public void MeasureMorpeh() {
            foreach (var index in morpehMap) {
                Volatile.Read(ref morpehMap.GetValueRefByIndex(index));
            }
        }
    }

    internal sealed class Add : IBenchmarkComparisonContainer {
        private Dictionary<int, int> bclMap;
        private IntHashMap<int> morpehMap;
        private List<int> keys;

        public void AllocBCL(int capacity) {
            this.bclMap = new Dictionary<int, int>(capacity);
            this.keys = CreateRandomUniqueKeys(capacity);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = new IntHashMap<int>(capacity);
            this.keys = CreateRandomUniqueKeys(capacity);
        }

        public void MeasureBCL() {
            foreach (var key in keys) {
                this.bclMap.Add(key, key);
            }
        }

        public void MeasureMorpeh() {
            foreach (var key in keys) {
                this.morpehMap.Add(key, key, out _);
            }
        }
    }

    internal sealed class AddGrow : IBenchmarkComparisonContainer {
        private Dictionary<int, int> bclMap;
        private IntHashMap<int> morpehMap;
        private List<int> values;

        public void AllocBCL(int capacity) {
            this.bclMap = new Dictionary<int, int>(4);
            this.values = CreateRandomUniqueKeys(capacity);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = new IntHashMap<int>(4);
            this.values = CreateRandomUniqueKeys(capacity);
        }

        public void MeasureBCL() {
            foreach (var value in values) {
                this.bclMap.Add(value, value);
            }
        }

        public void MeasureMorpeh() {
            foreach (var value in values) {
                this.morpehMap.Add(value, value, out _);
            }
        }
    }
}
#endif
