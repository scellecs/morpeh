using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class WorldTests : IDisposable {
    private readonly ITestOutputHelper output;

    public WorldTests(ITestOutputHelper output) {
        ClearStatic();
        this.output = output;
        MLogger.SetInstance(new XUnitLogger(this.output));
    }

    [Fact]
    public void DefaultWorld_AlwaysFirstInWorldsArray() {
        var initialDefaultWorld = World.Create();
        Assert.NotNull(initialDefaultWorld);
        Assert.Same(initialDefaultWorld, World.Default);
        Assert.Same(initialDefaultWorld, World.worlds[0]);

        var additionalWorlds = new World[10];
        for (int i = 0; i < additionalWorlds.Length; i++) {
            additionalWorlds[i] = World.Create();
        }

        Assert.Same(initialDefaultWorld, World.Default);
        Assert.Same(initialDefaultWorld, World.worlds[0]);

        initialDefaultWorld.Dispose();

        Assert.NotNull(World.Default);
        Assert.Same(World.worlds[0], World.Default);

        foreach (var world in additionalWorlds) {
            world.Dispose();
        }

        Assert.Null(World.Default);
        Assert.Equal(0, World.worldsCount);

        var newDefaultWorld = World.Create();
        Assert.NotNull(newDefaultWorld);
        Assert.Same(newDefaultWorld, World.Default);
        Assert.Same(newDefaultWorld, World.worlds[0]);
    }

    [Fact]
    public void InitializationDefaultWorld_CorrectlyCleanupWorlds() {
        for (int i = 0; i < 255; i++)  {
            World.Create();
        }

        Assert.Equal(255, World.worldsCount);

        WorldExtensions.InitializationDefaultWorld();
        Assert.NotNull(World.Default);
        Assert.Equal(1, World.worldsCount);
    }

    [Fact]
    public void WorldsIds_Limitation() {
        for (int i = 0; i < WorldConstants.MAX_WORLDS_COUNT; i++) {
            var world = World.Create();
            Assert.NotNull(world);
        }

        var extraWorld = World.Create();
        Assert.Null(extraWorld);
    }

    [Fact]
    public void WorldGenerations_Limitation() {
        var world = World.Create();
        var initialGeneration = world.generation;

        Assert.Equal(0, initialGeneration);

        for (int i = 0; i < 257; i++) {
            world.Dispose();
            world = World.Create();
            Assert.Equal((initialGeneration + i + 1) % 256, world.generation);
        }
    }

    [Fact]
    public void WorldGenerations_CorrectlyUse() {
        for (int i = 0; i < 255; i++) {
            World.Create();
        }

        Assert.Equal(0, World.Default.generation);
        WorldExtensions.InitializationDefaultWorld();
        Assert.Equal(1, World.Default.generation);
        World.Default.Dispose();
        World.Create();
        Assert.Equal(2, World.Default.generation);
        var world = World.Create();
        Assert.Equal(1, world.generation);
    }

    [Fact]
    public void WorldCreate_CorrectlySetIds() {
        WorldExtensions.InitializationDefaultWorld();
        Assert.NotNull(World.Default);
        Assert.Equal(0, World.Default.identifier);
        Assert.Equal(0, World.Default.generation);

        var world = World.Create();
        Assert.Equal(1, world.identifier);
        Assert.Equal(0, world.generation);

        World.Default.Dispose();
        Assert.Equal(world, World.Default);

        var world2 = World.Create();
        Assert.Equal(0, world2.identifier);
        Assert.Equal(1, world2.generation);
        Assert.NotEqual(world2, World.Default);
        Assert.Equal(0, World.Default.generation);

        world.Dispose();
        Assert.Equal(world2, World.Default);

        var world3 = World.Create();
        Assert.Equal(1, world3.identifier);
        Assert.Equal(1, world3.generation);
        Assert.NotEqual(world3, World.Default);

        world3.Dispose();

        var world4 = World.Create();
        Assert.Equal(1, world4.identifier);
        Assert.Equal(2, world4.generation);

        var world5 = World.Create();
        Assert.Equal(2, world5.identifier);
        Assert.Equal(0, world5.generation);
    }

    [Fact]
    public void GetWorld_GenerationCheck() {
        var world = World.Create();
        var entity = world.CreateEntity();
        var initialGen = World.worldsGens[world.identifier];

        Assert.Same(world, entity.GetWorld());
        Assert.False(world.IsDisposed(entity));
        Assert.False(entity.IsNullOrDisposed());
        Assert.False(entity.IsDisposed());

        for (int i = 0; i < 3; i++) {
            world.Dispose();
            Assert.Null(entity.GetWorld());
            Assert.True(world.IsDisposed(entity));
            Assert.True(entity.IsNullOrDisposed());
            Assert.True(entity.IsDisposed());

            world = World.Create();
            Assert.NotEqual(initialGen, World.worldsGens[world.identifier]);

            Assert.Null(entity.GetWorld());
            Assert.True(world.IsDisposed(entity));
            Assert.True(entity.IsNullOrDisposed());
            Assert.True(entity.IsDisposed());

            var newEntity = world.CreateEntity();
            Assert.Same(world, newEntity.GetWorld());
            Assert.False(world.IsDisposed(newEntity));
            Assert.False(newEntity.IsNullOrDisposed());
            Assert.False(newEntity.IsDisposed());
        }

        world.Dispose();
    }

    [Fact]
    public void GetWorld_ParallelWorlds()
    {
        var worlds = new World[WorldConstants.MAX_WORLDS_COUNT];
        var entities = new Entity[WorldConstants.MAX_WORLDS_COUNT];

        for (int i = 0; i < WorldConstants.MAX_WORLDS_COUNT; i++) {
            worlds[i] = World.Create();
            Assert.NotNull(worlds[i]);
            entities[i] = worlds[i].CreateEntity();
        }

        for (int i = 0; i < WorldConstants.MAX_WORLDS_COUNT; i++) {
            Assert.Same(worlds[i], entities[i].GetWorld());
            Assert.False(worlds[i].IsDisposed(entities[i]));
            Assert.False(entities[i].IsNullOrDisposed());
            Assert.False(entities[i].IsDisposed());
        }

        for (int i = WorldConstants.MAX_WORLDS_COUNT - 1; i >= 0; i--) {
            worlds[i].Dispose();
            Assert.Null(entities[i].GetWorld());
            Assert.True(worlds[i].IsDisposed(entities[i]));
            Assert.True(entities[i].IsNullOrDisposed());
            Assert.True(entities[i].IsDisposed());

            for (int j = 0; j < i; j++) {
                Assert.Same(worlds[j], entities[j].GetWorld());
                Assert.False(worlds[j].IsDisposed(entities[j]));
                Assert.False(entities[j].IsNullOrDisposed());
                Assert.False(entities[j].IsDisposed());
            }
        }
    }

    [Fact]
    public void GetWorld_BatchedCreateDispose() {
        var worlds = new List<World>();
        var entities = new List<(Entity entity, int worldIndex)>();

        // Create initial 10 worlds
        for (int i = 0; i < 10; i++) {
            var world = World.Create();
            worlds.Add(world);
            var entity = world.CreateEntity();
            entities.Add((entity, i));
        }

        // Check all entities are valid
        foreach (var (entity, index) in entities) {
            Assert.Same(worlds[index], entity.GetWorld());
            Assert.False(worlds[index].IsDisposed(entity));
            Assert.False(entity.IsNullOrDisposed());
            Assert.False(entity.IsDisposed());
        }

        // Remove 5 worlds
        for (int i = 0; i < 5; i++) {
            worlds[i].Dispose();
        }

        // Check entities - first 5 should be invalid, others valid
        foreach (var (entity, index) in entities) {
            if (index < 5) {
                Assert.Null(entity.GetWorld());
                Assert.True(worlds[index].IsDisposed(entity));
                Assert.True(entity.IsNullOrDisposed());
                Assert.True(entity.IsDisposed());
            }
            else {
                Assert.Same(worlds[index], entity.GetWorld());
                Assert.False(worlds[index].IsDisposed(entity));
                Assert.False(entity.IsNullOrDisposed());
                Assert.False(entity.IsDisposed());
            }
        }

        // Create 10 more worlds
        int prevCount = worlds.Count;
        for (int i = 0; i < 10; i++) {
            var world = World.Create();
            worlds.Add(world);
            var entity = world.CreateEntity();
            entities.Add((entity, prevCount + i));
        }

        // Check all entities - first 5 invalid, others valid
        foreach (var (entity, index) in entities) {
            if (index < 5) {
                Assert.Null(entity.GetWorld());
                Assert.True(worlds[index].IsDisposed(entity));
                Assert.True(entity.IsNullOrDisposed());
                Assert.True(entity.IsDisposed());
            }
            else {
                Assert.Same(worlds[index], entity.GetWorld());
                Assert.False(worlds[index].IsDisposed(entity));
                Assert.False(entity.IsNullOrDisposed());
                Assert.False(entity.IsDisposed());
            }
        }

        // Remove all except last world
        for (int i = 5; i < worlds.Count - 1; i++) {
            worlds[i].Dispose();
        }

        // Check entities - only entities from last world should be valid
        foreach (var (entity, index) in entities) {
            if (index == worlds.Count - 1) {
                Assert.Same(worlds[index], entity.GetWorld());
                Assert.False(worlds[index].IsDisposed(entity));
                Assert.False(entity.IsNullOrDisposed());
                Assert.False(entity.IsDisposed());
            }
            else {
                Assert.Null(entity.GetWorld());
                Assert.True(worlds[index].IsDisposed(entity));
                Assert.True(entity.IsNullOrDisposed());
                Assert.True(entity.IsDisposed());
            }
        }

        // Add 5 more worlds
        prevCount = worlds.Count;
        for (int i = 0; i < 5; i++) {
            var world = World.Create();
            worlds.Add(world);
            var entity = world.CreateEntity();
            entities.Add((entity, prevCount + i));
        }

        // Check entities - last 6 worlds should have valid entities
        foreach (var (entity, index) in entities) {
            if (index >= worlds.Count - 6) {
                Assert.Same(worlds[index], entity.GetWorld());
                Assert.False(worlds[index].IsDisposed(entity));
                Assert.False(entity.IsNullOrDisposed());
                Assert.False(entity.IsDisposed());
            }
            else {
                Assert.Null(entity.GetWorld());
                Assert.True(worlds[index].IsDisposed(entity));
                Assert.True(entity.IsNullOrDisposed());
                Assert.True(entity.IsDisposed());
            }
        }

        // Remove all worlds
        for (int i = worlds.Count - 6; i < worlds.Count; i++) {
            worlds[i].Dispose();
        }

        // Check all entities are invalid
        foreach (var (entity, index) in entities) {
            Assert.Null(entity.GetWorld());
            Assert.True(worlds[index].IsDisposed(entity));
            Assert.True(entity.IsNullOrDisposed());
            Assert.True(entity.IsDisposed());
        }

        // Create final 5 worlds
        worlds.Clear();
        var lastWorlds = new List<World>();
        for (int i = 0; i < 5; i++) {
            var world = World.Create();
            lastWorlds.Add(world);
            var entity = world.CreateEntity();
            entities.Add((entity, i));
        }

        // Check new entities are valid
        for (int i = entities.Count - 5; i < entities.Count; i++) {
            var (entity, index) = entities[i];
            Assert.Same(lastWorlds[index], entity.GetWorld());
            Assert.False(lastWorlds[index].IsDisposed(entity));
            Assert.False(entity.IsNullOrDisposed());
            Assert.False(entity.IsDisposed());
        }

        // Final cleanup
        foreach (var world in lastWorlds)
        {
            world.Dispose();
        }
    }

    [Fact]
    public void ThreadIdLock_GetSet() {
        var world = World.Create();
        var currentThreadId = Environment.CurrentManagedThreadId;

        Assert.Equal(currentThreadId, world.GetThreadId());

        var newThreadId = currentThreadId + 1;
        world.SetThreadId(newThreadId);
        Assert.Equal(newThreadId, world.GetThreadId());
    }

    [Fact]
    public void ThreadSafetyCheck_EnforcesThreadConstraint() {
#if MORPEH_THREAD_SAFETY
        var world = World.Create();
        world.SetThreadId(Environment.CurrentManagedThreadId + 1);
        Assert.Throws<ThreadSafetyCheckFailedException>(() => {
            world.ThreadSafetyCheck();
        });
#endif
    }

    [Fact]
    public void WarmupArchetypes_DoesNotThrows() {
        var world = World.Create();
        var warmupCount = 1 << 15;
        world.WarmupArchetypes(warmupCount);
    }

    [Fact]
    public void SystemsGroup_WorldAddRemove() {
        var world = World.Create();

        var systemsGroup1 = world.CreateSystemsGroup();
        var systemsGroup2 = world.CreateSystemsGroup();

        world.AddSystemsGroup(0, systemsGroup1);
        world.AddSystemsGroup(1, systemsGroup2);

        world.Update(0f);

        Assert.Contains(systemsGroup1, world.systemsGroups.Values);
        Assert.Contains(systemsGroup2, world.systemsGroups.Values);

        world.RemoveSystemsGroup(systemsGroup1);

        world.Update(0f);

        Assert.DoesNotContain(systemsGroup1, world.systemsGroups.Values);
        Assert.Contains(systemsGroup2, world.systemsGroups.Values);

        world.Dispose();
    }

    [Fact]
    public void SystemsGroup_SystemsAddRemove() {
        var world = World.Create();
        var systemsGroup = world.CreateSystemsGroup();

        var system1 = new TestUpdateSystem();
        var system2 = new TestFixedSystem();
        var system3 = new TestLateSystem();
        var system4 = new TestCleanupSystem();

        systemsGroup.AddSystem(system1);
        systemsGroup.AddSystem(system2);
        systemsGroup.AddSystem(system3);
        systemsGroup.AddSystem(system4);

        Assert.Contains(system1, systemsGroup.systems.data);
        Assert.Contains(system2, systemsGroup.fixedSystems.data);
        Assert.Contains(system3, systemsGroup.lateSystems.data);
        Assert.Contains(system4, systemsGroup.cleanupSystems.data);

        Assert.Equal(1, systemsGroup.systems.length);
        Assert.Equal(1, systemsGroup.fixedSystems.length);
        Assert.Equal(1, systemsGroup.lateSystems.length);
        Assert.Equal(1, systemsGroup.cleanupSystems.length);

        systemsGroup.RemoveSystem(system1);

        Assert.DoesNotContain(system1, systemsGroup.systems.data);
        Assert.NotEqual(1, systemsGroup.systems.length);
        Assert.Contains(system2, systemsGroup.fixedSystems.data);
        Assert.Contains(system3, systemsGroup.lateSystems.data);
        Assert.Contains(system4, systemsGroup.cleanupSystems.data);

        world.Dispose();
    }

    [Fact]
    public void SystemsGroup_ExceptionHandling() {
        var world = World.Create();
        var systemsGroup = world.CreateSystemsGroup();
        var pluginGroup = world.CreateSystemsGroup();

        var updateSystem = new TestUpdateSystem { throwException = false, updateCount = -1 };
        var fixedSystem = new TestFixedSystem { throwException = true };
        var lateSystem = new TestLateSystem { throwException = true };
        var cleanupSystem = new TestCleanupSystem { throwException = true };
        var pluginSystem = new TestUpdateSystem { throwException = false, updateCount = -1 };

        systemsGroup.AddSystem(updateSystem);
        systemsGroup.AddSystem(fixedSystem);
        systemsGroup.AddSystem(lateSystem);
        systemsGroup.AddSystem(cleanupSystem);
        pluginGroup.AddSystem(pluginSystem);

        world.AddSystemsGroup(0, systemsGroup);
        world.AddPluginSystemsGroup(pluginGroup);
        world.Update(0f);                       // InitializationUpdate
        updateSystem.throwException = true;
        pluginSystem.throwException = true;

        Assert.Equal(0, updateSystem.updateCount);
        Assert.Equal(0, fixedSystem.updateCount);
        Assert.Equal(0, lateSystem.updateCount);
        Assert.Equal(0, cleanupSystem.updateCount);
        Assert.Equal(0, pluginSystem.updateCount);
#if RELEASE
        Assert.Throws<ArgumentException>(() => world.FixedUpdate(1f));
        Assert.Throws<ArgumentException>(() => world.Update(1f));
        Assert.Throws<ArgumentException>(() => world.LateUpdate(1f));
        Assert.Throws<ArgumentException>(() => world.CleanupUpdate(1f));
#else
        world.FixedUpdate(1f);
        world.Update(1f);
        world.LateUpdate(1f);
        world.CleanupUpdate(1f);

        Assert.Equal(1, updateSystem.updateCount);
        Assert.Equal(1, fixedSystem.updateCount);
        Assert.Equal(1, lateSystem.updateCount);
        Assert.Equal(1, cleanupSystem.updateCount);
        Assert.Equal(1, pluginSystem.updateCount);

        world.FixedUpdate(1f);
        world.Update(1f);
        world.LateUpdate(1f);
        world.CleanupUpdate(1f);

        Assert.Equal(1, updateSystem.updateCount);
        Assert.Equal(1, fixedSystem.updateCount);
        Assert.Equal(1, lateSystem.updateCount);
        Assert.Equal(1, cleanupSystem.updateCount);
        Assert.Equal(1, pluginSystem.updateCount);

        Assert.DoesNotContain(updateSystem, systemsGroup.systems.data);
        Assert.DoesNotContain(fixedSystem, systemsGroup.fixedSystems.data);
        Assert.DoesNotContain(lateSystem, systemsGroup.lateSystems.data);
        Assert.DoesNotContain(cleanupSystem, systemsGroup.cleanupSystems.data);
        Assert.DoesNotContain(pluginSystem, pluginGroup.systems.data);

        Assert.Contains(updateSystem, systemsGroup.disabledSystems.data);
        Assert.Contains(fixedSystem, systemsGroup.disabledFixedSystems.data);
        Assert.Contains(lateSystem, systemsGroup.disabledLateSystems.data);
        Assert.Contains(cleanupSystem, systemsGroup.disabledCleanupSystems.data);
        Assert.Contains(pluginSystem, pluginGroup.disabledSystems.data);
#endif
        Assert.False(updateSystem.isDisposed);
        Assert.False(fixedSystem.isDisposed);
        Assert.False(lateSystem.isDisposed);
        Assert.False(cleanupSystem.isDisposed);
        Assert.False(pluginSystem.isDisposed);

        world.Dispose();

        Assert.True(updateSystem.isDisposed);
        Assert.True(fixedSystem.isDisposed);
        Assert.True(lateSystem.isDisposed);
        Assert.True(cleanupSystem.isDisposed);
        Assert.True(pluginSystem.isDisposed);
    }

    [Fact]
    public void SystemsGroup_ExceptionHandlingDoNotDisable() {
#if !RELEASE
        var world = World.Create();
        var systemsGroup = world.CreateSystemsGroup();
        var pluginGroup = world.CreateSystemsGroup();
        world.DoNotDisableSystemOnException = true;

        var updateSystem = new TestUpdateSystem { throwException = false, updateCount = -1 };
        var fixedSystem = new TestFixedSystem { throwException = true };
        var lateSystem = new TestLateSystem { throwException = true };
        var cleanupSystem = new TestCleanupSystem { throwException = true };
        var pluginSystem = new TestUpdateSystem { throwException = false, updateCount = -1 };

        systemsGroup.AddSystem(updateSystem);
        systemsGroup.AddSystem(fixedSystem);
        systemsGroup.AddSystem(lateSystem);
        systemsGroup.AddSystem(cleanupSystem);
        pluginGroup.AddSystem(pluginSystem);

        world.AddSystemsGroup(0, systemsGroup);
        world.AddPluginSystemsGroup(pluginGroup);
        world.Update(0f);                       // InitializationUpdate
        updateSystem.throwException = true;
        pluginSystem.throwException = true;

        Assert.Equal(0, updateSystem.updateCount);
        Assert.Equal(0, fixedSystem.updateCount);
        Assert.Equal(0, lateSystem.updateCount);
        Assert.Equal(0, cleanupSystem.updateCount);
        Assert.Equal(0, pluginSystem.updateCount);

        world.FixedUpdate(1f);
        world.Update(1f);
        world.LateUpdate(1f);
        world.CleanupUpdate(1f);

        Assert.Equal(1, updateSystem.updateCount);
        Assert.Equal(1, fixedSystem.updateCount);
        Assert.Equal(1, lateSystem.updateCount);
        Assert.Equal(1, cleanupSystem.updateCount);
        Assert.Equal(1, pluginSystem.updateCount);

        world.FixedUpdate(1f);
        world.Update(1f);
        world.LateUpdate(1f);
        world.CleanupUpdate(1f);

        Assert.Equal(2, updateSystem.updateCount);
        Assert.Equal(2, fixedSystem.updateCount);
        Assert.Equal(2, lateSystem.updateCount);
        Assert.Equal(2, cleanupSystem.updateCount);
        Assert.Equal(2, pluginSystem.updateCount);

        Assert.DoesNotContain(updateSystem, systemsGroup.disabledSystems.data);
        Assert.DoesNotContain(fixedSystem, systemsGroup.disabledFixedSystems.data);
        Assert.DoesNotContain(lateSystem, systemsGroup.disabledLateSystems.data);
        Assert.DoesNotContain(cleanupSystem, systemsGroup.disabledCleanupSystems.data);
        Assert.DoesNotContain(pluginSystem, pluginGroup.disabledSystems.data);

        Assert.Contains(updateSystem, systemsGroup.systems.data);
        Assert.Contains(fixedSystem, systemsGroup.fixedSystems.data);
        Assert.Contains(lateSystem, systemsGroup.lateSystems.data);
        Assert.Contains(cleanupSystem, systemsGroup.cleanupSystems.data);
        Assert.Contains(pluginSystem, pluginGroup.systems.data);

        Assert.False(updateSystem.isDisposed);
        Assert.False(fixedSystem.isDisposed);
        Assert.False(lateSystem.isDisposed);
        Assert.False(cleanupSystem.isDisposed);
        Assert.False(pluginSystem.isDisposed);

        world.Dispose();

        Assert.True(updateSystem.isDisposed);
        Assert.True(fixedSystem.isDisposed);
        Assert.True(lateSystem.isDisposed);
        Assert.True(cleanupSystem.isDisposed);
        Assert.True(pluginSystem.isDisposed);
#endif
    }
#pragma warning disable 0618
    [Fact]
    public void Commit_UpdateFilters() {
        var world = World.Create();
        var filter = world.Filter.With<Test1>().With<Test2>().Build();
        var filter2 = world.Filter.With<Test1>().Without<Test2>().Build();
        var entity = world.CreateEntity();

        entity.AddComponent<Test1>();

        Assert.True(filter.IsEmpty());
        Assert.True(filter2.IsEmpty());

        world.Commit();

        Assert.True(filter.IsEmpty());
        Assert.True(filter2.IsNotEmpty());

        entity.AddComponent<Test2>();

        Assert.True(filter.IsEmpty());
        Assert.True(filter2.IsNotEmpty());

        world.Commit();

        Assert.True(filter.IsNotEmpty());
        Assert.True(filter2.IsEmpty());
    }
#pragma warning restore 0618
    [Fact]
    public void DisposeAndIsNullOrDisposed_Behavior() {
        var world = World.Create();
        Assert.False(world.IsNullOrDisposed());

        world.Dispose();
        Assert.True(world.IsNullOrDisposed());

        world.Dispose();
    }

    public void Dispose() {
        ClearStatic();
    }

    private static void ClearStatic() {
        for (int i = World.worldsCount - 1; i >= 0; i--) {
            var world = World.worlds[i];
            if (!world.IsNullOrDisposed()) {
                world.Dispose();
            }
        }

        World.defaultWorld = null;
        World.worlds = new World[WorldConstants.MAX_WORLDS_COUNT];
        World.worldsIndices = new int[WorldConstants.MAX_WORLDS_COUNT];
        World.plugins?.Clear();
        World.worldsCount = 0;
        World.worldsGens = new byte[WorldConstants.MAX_WORLDS_COUNT];

        Assert.Null(World.Default);
        Assert.Equal(0, World.worldsCount);
        Assert.True(World.worlds.All(x => x == null));
        Assert.True(World.worldsGens.All(x => x == 0));
    }
}