using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class StashTests {
    private readonly World world;
    
    public StashTests(ITestOutputHelper output) {
        world = World.Create();
        MLogger.SetInstance(new XUnitLogger(output));
    }

    [Fact]
    public void RemoveClearsData() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();

        stash.Set(entity, new IntTest1 {
            value = 123,
        });
        
        ref var data = ref stash.Get(entity);
        Assert.Equal(123, data.value);
        
        stash.Remove(entity);
        
        Assert.False(stash.Has(entity));
        Assert.Equal(default, data.value);

        this.world.RemoveEntity(entity);
        this.world.Commit();
    }
    
    [Fact]
    public void EntityDisposeClearsData() {
        var entity = this.world.CreateEntity();
        var stash  = this.world.GetStash<IntTest1>();

        stash.Set(entity, new IntTest1 {
            value = 123,
        });
        
        ref var data = ref stash.Get(entity);
        Assert.Equal(123, data.value);
        
        this.world.RemoveEntity(entity);
        this.world.Commit();
        
        Assert.Equal(default, data.value);
    }
}