using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class EntityDisposalTests {
    private readonly ITestOutputHelper output;
    private readonly World world;
    private readonly TagStash tagTest1;
    
    public EntityDisposalTests(ITestOutputHelper output) {
        this.output = output;
        MLogger.SetInstance(new XUnitLogger(this.output));
        
        this.world = World.Create();
        this.tagTest1 = TagTest1.GetStash(this.world);
    }
    
    [Fact]
    public void DisposeEmptyEntity() {
        var entity = this.world.CreateEntity();
        Assert.Equal(1, world.entitiesCount);
        
        this.world.RemoveEntity(entity);
        Assert.True(this.world.IsDisposed(entity));
        Assert.Equal(0, world.entitiesCount);
        
        var newEntity = this.world.CreateEntity();
        Assert.NotEqual(newEntity, entity);
        Assert.NotEqual(entity.Id, newEntity.Id);
    }
    
    [Fact]
    public void DisposeNonEmptyEntity() {
        var entity = this.world.CreateEntity();
        
        this.tagTest1.Set(entity);
        Assert.Equal(1, world.entitiesCount);
        
        this.world.RemoveEntity(entity);
        Assert.True(this.world.IsDisposed(entity));
        Assert.Equal(0, world.entitiesCount);
        
        var newEntity = this.world.CreateEntity();
        Assert.NotEqual(newEntity, entity);
        Assert.NotEqual(entity.Id, newEntity.Id);
    }
    
    [Fact]
    public void DisposeEmptyEntityWithCommit() {
        var entity = this.world.CreateEntity();
        Assert.Equal(1, world.entitiesCount);
        
        this.world.RemoveEntity(entity);
        this.world.Commit();
        Assert.True(this.world.IsDisposed(entity));
        Assert.Equal(0, world.entitiesCount);
        
        var newEntity = this.world.CreateEntity();
        Assert.NotEqual(newEntity, entity);
        Assert.Equal(entity.Generation + 1, newEntity.Generation);
    }
    
    [Fact]
    public void DisposeNonEmptyEntityWithCommit() {
        var entity = this.world.CreateEntity();
        Assert.Equal(1, world.entitiesCount);
        
        this.tagTest1.Set(entity);
        this.world.RemoveEntity(entity);
        this.world.Commit();
        Assert.True(this.world.IsDisposed(entity));
        Assert.Equal(0, world.entitiesCount);
        
        
        var newEntity = this.world.CreateEntity();
        Assert.NotEqual(newEntity, entity);
        Assert.Equal(entity.Generation + 1, newEntity.Generation);
    }
    
    [Fact]
    public void DisposeNonEmptyEntityCommitAfterEachOp() {
        var entity = this.world.CreateEntity();
        Assert.Equal(1, world.entitiesCount);
        
        this.tagTest1.Set(entity);
        this.world.Commit();
        Assert.False(this.world.IsDisposed(entity));
        Assert.Equal(1, world.entitiesCount);
        
        this.world.RemoveEntity(entity);
        Assert.Equal(0, world.entitiesCount);
        Assert.True(this.world.IsDisposed(entity));
        
        this.world.Commit();
        Assert.True(this.world.IsDisposed(entity));
        Assert.Equal(0, world.entitiesCount);
        
        var newEntity = this.world.CreateEntity();
        Assert.NotEqual(newEntity, entity);
        Assert.Equal(entity.Generation + 1, newEntity.Generation);
    }
    
    [Fact]
    public void RemoveLastComponent() {
        var entity = this.world.CreateEntity();
        Assert.Equal(1, world.entitiesCount);
        
        this.tagTest1.Set(entity);
        this.tagTest1.Remove(entity);
        Assert.False(this.world.IsDisposed(entity));
        Assert.Equal(1, world.entitiesCount);
        
        this.world.Commit();
        
        Assert.True(this.world.IsDisposed(entity));
        Assert.Equal(0, world.entitiesCount);
        
        var newEntity = this.world.CreateEntity();
        Assert.NotEqual(newEntity, entity);
        Assert.Equal(entity.Generation + 1, newEntity.Generation);
    }
    
    [Fact]
    public void RemoveLastComponentWithCommits() {
        var entity = this.world.CreateEntity();
        Assert.Equal(1, world.entitiesCount);
        
        this.tagTest1.Set(entity);
        this.world.Commit();
        Assert.Equal(1, world.entitiesCount);
        
        this.tagTest1.Remove(entity);
        this.world.Commit();
        
        Assert.True(this.world.IsDisposed(entity));
        Assert.Equal(0, world.entitiesCount);
        
        var newEntity = this.world.CreateEntity();
        Assert.NotEqual(newEntity, entity);
        Assert.Equal(entity.Generation + 1, newEntity.Generation);
    }
}