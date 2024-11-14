using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class WorldTests {
    private readonly ITestOutputHelper output;

    public WorldTests(ITestOutputHelper output) {
        this.output = output;
        MLogger.SetInstance(new XUnitLogger(this.output));
    }

    [Fact]
    public void InitializationDefaultWorldCorrectlyCleanupWorlds() {
        ClearStatic();

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
    public void WorldsCorrectlyUseGens() {
        ClearStatic();

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
    public void WorldsReuseIds() {
        ClearStatic();

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

    private static void ClearStatic() {
        for (int i = World.worlds.length - 1; i >= 0; i--) {
            var world = World.worlds.data[i];
            if (!world.IsNullOrDisposed()) {
                world.Dispose();
            }
        }

        World.defaultWorld = null;
        World.worlds = new FastList<World>();
        World.freeWorldIDs = new IntStack();
        World.plugins?.Clear();
        World.worldsCount = 0;
        World.worldsGens = new byte[4];

        AssertStaticClean();
    }

    private static void AssertStaticClean() {
        Assert.Null(World.Default);
        Assert.Equal(0, World.freeWorldIDs.length);
        Assert.Equal(0, World.worlds.length);
        Assert.True(World.worlds.data.All(x => x == null));
        Assert.True(World.worldsGens.All(x => x == 0));
    }
}