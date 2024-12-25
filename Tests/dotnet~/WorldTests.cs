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
        Assert.Same(initialDefaultWorld, World.worlds.data[0]);

        var additionalWorlds = new World[10];
        for (int i = 0; i < additionalWorlds.Length; i++) {
            additionalWorlds[i] = World.Create();
        }

        Assert.Same(initialDefaultWorld, World.Default);
        Assert.Same(initialDefaultWorld, World.worlds.data[0]);

        initialDefaultWorld.Dispose();

        Assert.NotNull(World.Default);
        Assert.Same(additionalWorlds[0], World.Default);
        Assert.Same(additionalWorlds[0], World.worlds.data[0]);

        foreach (var world in additionalWorlds) {
            world.Dispose();
        }

        Assert.Null(World.Default);
        Assert.Equal(0, World.worlds.length);

        var newDefaultWorld = World.Create();
        Assert.NotNull(newDefaultWorld);
        Assert.Same(newDefaultWorld, World.Default);
        Assert.Same(newDefaultWorld, World.worlds.data[0]);
    }

    [Fact]
    public void InitializationDefaultWorld_CorrectlyCleanupWorlds() {
        for (int i = 0; i < 255; i++)  {
            World.Create();
        }

        Assert.Equal(255, World.worldsCount);
        Assert.Equal(255, World.worlds.length);

        WorldExtensions.InitializationDefaultWorld();
        Assert.NotNull(World.Default);
        Assert.Equal(1, World.worldsCount);
        Assert.Equal(1, World.worlds.length);
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
    public void WorldIds_Reuse() {
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

#pragma warning disable 0618
    [Fact]
    public void Commit_UpdateFilters() {
        var world = World.Create();
        var filter = world.Filter.With<TagTest1>().With<TagTest2>().Build();
        var filter2 = world.Filter.With<TagTest1>().Without<TagTest2>().Build();
        var entity = world.CreateEntity();

        entity.AddComponent<TagTest1>();

        Assert.True(filter.IsEmpty());
        Assert.True(filter2.IsEmpty());

        world.Commit();

        Assert.True(filter.IsEmpty());
        Assert.True(filter2.IsNotEmpty());

        entity.AddComponent<TagTest2>();

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
        Assert.Equal(World.worlds.length, World.worldsCount);

        for (int i = World.worlds.length - 1; i >= 0; i--) {
            var world = World.worlds.data[i];
            if (!world.IsNullOrDisposed()) {
                world.Dispose();
            }
        }

        World.defaultWorld = null;
        World.worlds = new FastList<World>();
        World.freeWorldIDs = new IntStack();
        World.worldsCount = 0;
        World.worldsGens = new byte[4];

        Assert.Null(World.Default);
        Assert.Equal(0, World.freeWorldIDs.length);
        Assert.Equal(0, World.worlds.length);
        Assert.True(World.worlds.data.All(x => x == null));
        Assert.True(World.worldsGens.All(x => x == 0));
    }
}