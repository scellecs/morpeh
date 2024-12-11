using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class EntityIdReuseTests {
    private readonly ITestOutputHelper output;
    
    public EntityIdReuseTests(ITestOutputHelper output) {
        this.output = output;
        MLogger.SetInstance(new XUnitLogger(this.output));
    }

    [Fact]
    public void CommitReusesEntityIds() {
        const int count = 128;
        
        using var world = World.Create();

        var entities = new Entity[count];
        
        for (var i = 0; i < count; i++) {
            entities[i] = world.CreateEntity();
            Assert.Equal(i + 1, entities[i].Id);
        }

        for (var i = 0; i < count; i++) {
            world.RemoveEntity(entities[i]);
        }
        
        world.Commit();
        
        for (var i = 0; i < count; i++) {
            Assert.Equal(i + 1, world.CreateEntity().Id);
        }
    }
    
    [Fact]
    public void NoCommitDoesntReuseEntityIds() {
        const int count = 128;
        
        using var world = World.Create();

        var entities = new Entity[count];
        
        for (var i = 0; i < count; i++) {
            entities[i] = world.CreateEntity();
            Assert.Equal(i + 1, entities[i].Id);
        }

        for (var i = 0; i < count; i++) {
            world.RemoveEntity(entities[i]);
        }
        
        for (var i = 0; i < count; i++) {
            Assert.Equal(count + i + 1, world.CreateEntity().Id);
        }
    }
}