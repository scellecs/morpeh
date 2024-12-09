#if UNITY_EDITOR && MORPEH_BURST
using NUnit.Framework;
using Unity.Burst;
using Unity.Jobs;
using Scellecs.Morpeh.Native;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Scellecs.Morpeh.Tests.Unity.NativeStash {
    public unsafe class NativeStashTests {
        [Test]
        public unsafe void AsNative_WorksCorrectly() {
            var count = 100_000;
            var world = World.Create();
            var stash = world.GetStash<TestComponent0>();

            for (int i = 0; i < count; i++) {
                stash.Add(world.CreateEntity(), new TestComponent0() { value = i });
            }

            ref var empty = ref stash.empty;
            empty.value = -999;

            var nativeStash = stash.AsNative();

            Assert.IsTrue(nativeStash.map.buckets != null);
            Assert.IsTrue(nativeStash.map.slots != null);
            Assert.IsTrue(nativeStash.map.capacityMinusOnePtr != null);
            Assert.IsTrue(nativeStash.data != null);
            Assert.IsTrue(nativeStash.empty != null);

            Assert.AreEqual(stash.empty.value, nativeStash.empty->value);
            Assert.AreEqual(stash.map.capacityMinusOne, *nativeStash.map.capacityMinusOnePtr);
        }

        [Test]
        public void Get_WorksCorrectly() {
            var count = 10_000;
            var world = World.Create();
            var entities = new Entity[count];
            var stash = world.GetStash<TestComponent0>();
            var sum = 0;

            for (int i = 0; i < count; i++) {
                var ent = world.CreateEntity();
                stash.Add(ent, new TestComponent0() { value = i });
                entities[i] = ent;
                sum += i;
            }

            fixed (Entity* entitiesPtr = &entities[0]) {
                var resultReference = new NativeArray<int>(1, Allocator.TempJob, NativeArrayOptions.ClearMemory);
                new Get_WorksCorrectlyJob() {
                    stash = stash.AsNative(),
                    entities = entitiesPtr,
                    result = (int*)resultReference.GetUnsafePtr(),
                }
                .ScheduleParallel(count, 64, default).Complete();

                Assert.AreEqual(resultReference[0], sum);
                resultReference.Dispose(default);
            }
        }

        [Test]
        public void GetOutExists_WorksCorrectly() {
            var world = World.Create();
            var stash0 = world.GetStash<TestComponent0>();
            var stash1 = world.GetStash<TestComponent1>();

            var count = 3;
            var entities = new Entity[count];

            entities[0] = world.CreateEntity();
            stash0.Add(entities[0], new TestComponent0 { value = 42 });
            stash1.Add(entities[0], new TestComponent1 { value = 0 });

            entities[1] = world.CreateEntity();
            stash1.Add(entities[1], new TestComponent1 { value = 0 });

            entities[2] = world.CreateEntity();
            stash0.Add(entities[2], new TestComponent0 { value = 52 });
            stash1.Add(entities[2], new TestComponent1 { value = 0 });

            fixed (Entity* entitiesPtr = &entities[0]) {
                new GetOutExists_WorksCorrectlyJob() {
                    stash0 = stash0.AsNative(),
                    stash1 = stash1.AsNative(),
                    entities = entitiesPtr,
                    count = count,
                }
                .Schedule().Complete();

                Assert.IsTrue(stash0.Has(entities[0]));
                Assert.AreEqual(42, stash1.Get(entities[0]).value);

                Assert.IsFalse(stash0.Has(entities[1]));
                Assert.AreEqual(1, stash1.Get(entities[1]).value);

                Assert.IsTrue(stash0.Has(entities[2]));
                Assert.AreEqual(52, stash1.Get(entities[2]).value);
            }
        }

        [Test]
        public void Get_And_GetOutExists_ModificationsAreCorrect() {
            var count = 1000;
            var world = World.Create();
            var entities = new Entity[count];
            var stash0 = world.GetStash<TestComponent0>();
            var stash1 = world.GetStash<TestComponent1>();

            for (int i = 0; i < count; i++) {
                var ent = world.CreateEntity();
                entities[i] = ent;

                if (i % 3 == 0) {
                    stash0.Add(ent);
                    stash1.Add(ent);
                }
                else if (i % 4 == 0) {
                    stash0.Add(ent);
                }
                else if (i % 5 == 0) {
                    stash1.Add(ent);
                }
            }

            fixed (Entity* entitiesPtr = &entities[0]) {
                new GetModifications_WorksCorrectlyJob {
                    stash0 = stash0.AsNative(),
                    stash1 = stash1.AsNative(),
                    entities = entitiesPtr,
                }
                .ScheduleParallel(count, 32, default).Complete();
            }

            for (int i = 0; i < count; i++) {
                var ent = entities[i];

                if (stash0.Has(ent)) {
                    Assert.AreEqual(ent.Id, stash0.Get(ent).value);
                }

                if (stash1.Has(ent)) {
                    Assert.AreEqual(ent.Id, stash1.Get(ent).value);
                }
            }
        }

        [Test]
        public void Has_WorksCorrectly() {
            var world = World.Create();
            var stash0 = world.GetStash<TestComponent0>();
            var stash1 = world.GetStash<TestComponent1>();
            var count = 10_000;
            var entities = new Entity[count];
            var component0Count = 0;

            for (int i = 0; i < count; i++) {
                var ent = world.CreateEntity();
                entities[i] = ent;

                if (i % 4 == 0) {
                    stash1.Add(ent, new TestComponent1 { value = i });
                }
                else {
                    stash0.Add(ent, new TestComponent0 { value = i });
                    component0Count++;
                }
            }

            fixed (Entity* entitiesPtr = &entities[0]) {
                var results = new NativeArray<int>(1, Allocator.TempJob, NativeArrayOptions.ClearMemory);
                new Has_WorksCorrectlyJob() {
                    stash = stash0.AsNative(),
                    entities = entitiesPtr,
                    result = (int*)results.GetUnsafePtr(),
                    count = count
                }
                .Schedule().Complete();

                Assert.AreEqual(component0Count, results[0]);
                results.Dispose(default);
            }
        }
    }

    [BurstCompile]
    public unsafe struct Get_WorksCorrectlyJob : IJobFor {
        public NativeStash<TestComponent0> stash;

        [NativeDisableUnsafePtrRestriction]
        public Entity* entities;

        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute(int index) {
            var ent = this.entities[index];
            ref var component = ref this.stash.Get(ent);
            Interlocked.Add(ref *this.result, component.value);
        }
    }

    [BurstCompile]
    public unsafe struct GetOutExists_WorksCorrectlyJob : IJob {
        public NativeStash<TestComponent0> stash0;
        public NativeStash<TestComponent1> stash1;
        public int count;

        [NativeDisableUnsafePtrRestriction]
        public Entity* entities;

        public void Execute() {
            for (int i = 0; i < count; i++) {
                var ent = this.entities[i];
                ref var component0 = ref this.stash0.Get(ent, out bool component0Exists);
                ref var component1 = ref this.stash1.Get(ent);
                component1.value = component0Exists ? component0.value : 1;
            }
        }
    }

    [BurstCompile]
    public unsafe struct GetModifications_WorksCorrectlyJob : IJobFor {
        public NativeStash<TestComponent0> stash0;
        public NativeStash<TestComponent1> stash1;

        [NativeDisableUnsafePtrRestriction]
        public Entity* entities;

        public void Execute(int index) {
            var ent = this.entities[index];

            ref var component0 = ref this.stash0.Get(ent);
            component0.value = ent.Id;

            ref var component1 = ref this.stash1.Get(ent, out bool exists);
            component1.value = ent.Id;
        }
    }

    [BurstCompile]
    public unsafe struct Has_WorksCorrectlyJob : IJob {
        public NativeStash<TestComponent0> stash;
        public int count;

        [NativeDisableUnsafePtrRestriction]
        public Entity* entities;

        [NativeDisableUnsafePtrRestriction]
        public int* result;

        public void Execute() {
            for (int i = 0; i < count; i++) {
                var ent = this.entities[i];
                if (this.stash.Has(ent)) {
                    (*this.result)++;
                }
            }
        }
    }
}
#endif
