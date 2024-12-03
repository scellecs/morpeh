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
using static Scellecs.Morpeh.Benchmarks.Collections.IntStack.IntStackTestsUtility;

namespace Scellecs.Morpeh.Benchmarks.Collections.IntStack {
    using IntStack = Scellecs.Morpeh.Collections.IntStack;

    [BenchmarkComparison(typeof(BenchmarkContainerType), "IntStack", "Stack")]
    internal sealed class IntStackPerformanceTests {
        //[Test, Performance]
        [Category("Performance")]
        public void Push([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<Push>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void PushGrow([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<PushGrow>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void Pop([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<Pop>(count, type);
        }

        //[Test, Performance]
        [Category("Performance")]
        public void TryPop([Values(10_000, 100_000, 1_000_000)] int count, [Values] BenchmarkContainerType type) {
            BenchmarkContainerRunner.RunComparison<TryPop>(count, type);
        }
    }

    internal static class IntStackTestsUtility {
        public static Stack<int> InitBCL(int capacity, bool addValues) {
            var stack = new Stack<int>(capacity);
            
            if (!addValues) {
                return stack;
            }
                
            for (int i = 0; i < capacity; i++) {
                stack.Push(i);
            }
            return stack;
        }
        
        public static IntStack InitMorpeh(int capacity, bool addValues) {
            var stack = new IntStack(capacity);
            
            if (!addValues) {
                return stack;
            }
                
            for (int i = 0; i < capacity; i++) {
                stack.Push(i);
            }
            return stack;
        }
    }

    internal sealed class Push : IBenchmarkComparisonContainer {
        private Stack<int> bclStack;
        private IntStack morpehStack;
        private int count;

        public void AllocBCL(int capacity) {
            this.bclStack = InitBCL(capacity, false);
            this.count = capacity;
        }

        public void AllocMorpeh(int capacity) {
            this.morpehStack = InitMorpeh(capacity, false);
            this.count = capacity;
        }

        public void MeasureBCL() {
            for (int i = 0; i < count; i++) {
                this.bclStack.Push(i);
            }
        }

        public void MeasureMorpeh() {
            for (int i = 0; i < count; i++) {
                this.morpehStack.Push(i);
            }
        }
    }

    internal sealed class PushGrow : IBenchmarkComparisonContainer {
        private Stack<int> bclStack;
        private IntStack morpehStack;
        private int count;

        public void AllocBCL(int capacity) {
            this.bclStack = new Stack<int>(4);
            this.count = capacity;
        }

        public void AllocMorpeh(int capacity) {
            this.morpehStack = new IntStack(4);
            this.count = capacity;
        }

        public void MeasureBCL() {
            for (int i = 0; i < count; i++) {
                this.bclStack.Push(i);
            }
        }

        public void MeasureMorpeh() {
            for (int i = 0; i < count; i++) {
                this.morpehStack.Push(i);
            }
        }
    }

    internal sealed class Pop : IBenchmarkComparisonContainer {
        private Stack<int> bclStack;
        private IntStack morpehStack;
        private int count;

        public void AllocBCL(int capacity) {
            this.bclStack = InitBCL(capacity, true);
            this.count = capacity;
        }

        public void AllocMorpeh(int capacity) {
            this.morpehStack = InitMorpeh(capacity, true);
            this.count = capacity;
        }

        public void MeasureBCL() {
            for (int i = 0; i < count; i++) {
                var value = this.bclStack.Pop();
                Volatile.Read(ref value);
            }
        }

        public void MeasureMorpeh() {
            for (int i = 0; i < count; i++) {
                var value = this.morpehStack.Pop();
                Volatile.Read(ref value);
            }
        }
    }

    internal sealed class TryPop : IBenchmarkComparisonContainer {
        private Stack<int> bclStack;
        private IntStack morpehStack;
        private int count;

        public void AllocBCL(int capacity) {
            this.bclStack = InitBCL(capacity, true);
            this.count = capacity;
        }

        public void AllocMorpeh(int capacity) {
            this.morpehStack = InitMorpeh(capacity, true);
            this.count = capacity;
        }

        public void MeasureBCL() {
            for (int i = 0; i < count; i++) {
                this.bclStack.TryPop(out var value);
                Volatile.Read(ref value);
            }
        }

        public void MeasureMorpeh() {
            for (int i = 0; i < count; i++) {
                this.morpehStack.TryPop(out var value);
                Volatile.Read(ref value);
            }
        }
    }
}
#endif