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
using static Scellecs.Morpeh.Benchmarks.Collections.LongHashMap.LongHashMapTestsUtility;

namespace Scellecs.Morpeh.Benchmarks.Collections.LongHashMap {
    [BenchmarkComparison(typeof(BenchmarkContainerType), "LongHashMap", "Dictionary")]
    internal sealed class LongHashMapPerformanceTests {
        //[Test, Performance]
        [Category("Performance")]
        public void IndexerRead([Values(10_000, 100_000, 1_000_000)] int capacity, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<IndexerRead>(capacity, type);
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
        public void TryGetValue([Values(10_000, 100_000, 1_000_000)] int capacity, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<TryGetValue>(capacity, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void ContainsKey([Values(10_000, 100_000, 1_000_000)] int capacity, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<ContainsKey>(capacity, type);
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

    internal static class LongHashMapTestsUtility {
        public static Dictionary<long, int> InitBCL(int capacity, bool addValues, out List<long> keys) {
            var map = new Dictionary<long, int>(capacity);
            keys = new List<long>(capacity);
        
            if (!addValues) {
                return map;
            }
            
            var random = new Random(52);
            int added = 0;
            while (added < capacity) {
                var key = random.NextLong(long.MaxValue);
                if (map.TryAdd(key, added)) {
                    keys.Add(key);
                    added++;
                }
            }
            Shuffle(keys);
            return map;
        }
    
        public static LongHashMap<int> InitMorpeh(int capacity, bool addValues, out List<long> keys) {
            var map = new LongHashMap<int>(capacity);
            keys = new List<long>(capacity);
        
            if (!addValues) {
                return map;
            }
            
            var random = new Random(52);
            int added = 0;
            while (added < capacity) {
                var key = random.NextLong(long.MaxValue);
                if (map.Add(key, added, out _)) {
                    keys.Add(key);
                    added++;
                }
            }
            Shuffle(keys);
            return map;
        }

        public static List<long> CreateRandomUniqueKeys(int capacity) {
            var values = new List<long>(capacity);
            var set = new HashSet<long>();
            var random = new Random(69);
            while (values.Count < capacity) {
                var value = random.NextLong(long.MaxValue);
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
        
        internal static long NextLong(this Random random) {
            Span<byte> bytes = stackalloc byte[8];
            random.NextBytes(bytes);
            bytes[7] &= 0x7F;
        
            return BitConverter.ToInt64(bytes);
        }

        internal static long NextLong(this Random random, long maxValue) {
            var result = random.NextLong();
            return result % maxValue;
        }
    }

    internal sealed class IndexerRead : IBenchmarkComparisonContainer {
        private Dictionary<long, int> bclMap;
        private LongHashMap<int> morpehMap;
        private List<long> keys;

        public void AllocBCL(int capacity) {
            this.bclMap = InitBCL(capacity, true, out this.keys);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = InitMorpeh(capacity, true, out this.keys);
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

    internal sealed class ContainsKey : IBenchmarkComparisonContainer {
        private Dictionary<long, int> bclMap;
        private LongHashMap<int> morpehMap;
        private List<long> keys;

        public void AllocBCL(int capacity) {
            this.bclMap = InitBCL(capacity, true, out this.keys);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = InitMorpeh(capacity, true, out this.keys);
        }

        public void MeasureBCL() {
            var result = false;
            for (int i = 0; i < keys.Count; i++) {
                Volatile.Write(ref result, this.bclMap.ContainsKey(this.keys[i]));
            }
        }

        public void MeasureMorpeh() {
            var result = false;
            for (int i = 0; i < keys.Count; i++) {
                Volatile.Write(ref result, this.morpehMap.Has(this.keys[i]));
            }
        }
    }

    internal sealed class TryGetValue : IBenchmarkComparisonContainer {
        private Dictionary<long, int> bclMap;
        private LongHashMap<int> morpehMap;
        private List<long> keys;

        public void AllocBCL(int capacity) {
            this.bclMap = InitBCL(capacity, true, out this.keys);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = InitMorpeh(capacity, true, out this.keys);
        }

        public void MeasureBCL() {
            for (int i = 0; i < keys.Count; i++) {
                this.bclMap.TryGetValue(this.keys[i], out var value);
                Volatile.Read(ref value);
            }
        }

        public void MeasureMorpeh() {
            for (int i = 0; i < keys.Count; i++) {
                this.morpehMap.TryGetValue(this.keys[i], out var value);
                Volatile.Read(ref value);
            }
        }
    }

    internal sealed class Remove : IBenchmarkComparisonContainer {
        private Dictionary<long, int> bclMap;
        private LongHashMap<int> morpehMap;
        private List<long> keys;

        public void AllocBCL(int capacity) {
            this.bclMap = InitBCL(capacity, true, out this.keys);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = InitMorpeh(capacity, true, out this.keys);
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
        private Dictionary<long, int> bclMap;
        private LongHashMap<int> morpehMap;

        public void AllocBCL(int capacity) {
            this.bclMap = InitBCL(capacity, true, out _);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = InitMorpeh(capacity, true, out _);
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
        private Dictionary<long, int> bclMap;
        private LongHashMap<int> morpehMap;
        private List<long> keys;

        public void AllocBCL(int capacity) {
            this.bclMap = InitBCL(capacity, false, out _);
            this.keys = CreateRandomUniqueKeys(capacity);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = InitMorpeh(capacity, false, out _);
            this.keys = CreateRandomUniqueKeys(capacity);
        }

        public void MeasureBCL() {
            for (int i = 0; i < keys.Count; i++) {
                var key = keys[i];
                this.bclMap.Add(key, i);
            }
        }

        public void MeasureMorpeh() {
            for (int i = 0; i < keys.Count; i++) {
                var key = keys[i];
                this.morpehMap.Add(key, i, out _);
            }
        }
    }

    internal sealed class AddGrow : IBenchmarkComparisonContainer {
        private Dictionary<long, int> bclMap;
        private LongHashMap<int> morpehMap;
        private List<long> values;

        public void AllocBCL(int capacity) {
            this.bclMap = new Dictionary<long, int>(4);
            this.values = CreateRandomUniqueKeys(capacity);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = new LongHashMap<int>(4);
            this.values = CreateRandomUniqueKeys(capacity);
        }

        public void MeasureBCL() {
            for (int i = 0; i < values.Count; i++) {
                var value = values[i];
                this.bclMap.Add(value, i);
            }
        }

        public void MeasureMorpeh() {
            for (int i = 0; i < values.Count; i++) {
                var value = values[i];
                this.morpehMap.Add(value, i, out _);
            }
        }
    }

    internal sealed class Clear : IBenchmarkComparisonContainer {
        private Dictionary<long, int> bclMap;
        private LongHashMap<int> morpehMap;

        public void AllocBCL(int capacity) {
            this.bclMap = InitBCL(capacity, true, out _);
        }

        public void AllocMorpeh(int capacity) {
            this.morpehMap = InitMorpeh(capacity, true, out _);
        }

        public void MeasureBCL() {
            this.bclMap.Clear();
        }

        public void MeasureMorpeh() {
            this.morpehMap.Clear();
        }
    }
}
#endif