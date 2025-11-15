using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class EntityDisposalTests {
    private readonly ITestOutputHelper output;
    private readonly World world;
    private readonly Stash<Test1> test1;
    
    public EntityDisposalTests(ITestOutputHelper output) {
        this.output = output;
        MLogger.SetInstance(new XUnitLogger(this.output));
        
        this.world = World.Create();
        this.test1 = this.world.GetStash<Test1>();
    }
    
    [Fact]
    public void DisposeEmptyEntity() {
        Entity entity = this.world.CreateEntity();
        Assert.Equal(1, this.world.entitiesCount);

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
        
        this.test1.Set(entity, new Test1());
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
        
        this.test1.Set(entity, new Test1());
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
        
        this.test1.Set(entity, new Test1());
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
        
        this.test1.Set(entity, new Test1());
        this.test1.Remove(entity);
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
        
        this.test1.Set(entity, new Test1());
        this.world.Commit();
        Assert.Equal(1, world.entitiesCount);
        
        this.test1.Remove(entity);
        this.world.Commit();
        
        Assert.True(this.world.IsDisposed(entity));
        Assert.Equal(0, world.entitiesCount);
        
        var newEntity = this.world.CreateEntity();
        Assert.NotEqual(newEntity, entity);
        Assert.Equal(entity.Generation + 1, newEntity.Generation);
    }

    [Fact]
    public void DisposeWithMonoProviderDoesNotDoubleRemoveFromStash() {
        var stashDisposable = this.world.GetStash<PooledObjectView>().AsDisposable();

        var entity = this.world.CreateEntity();

        var go = new ActivableGameObject();

        // Simulate a provider that adds/removes a component on activation/deactivation
        go.onActivate = () => {
            this.test1.Set(entity, new Test1());
        };
        go.onDeactivate = () => {
            if (this.world.Has(entity)) {
                this.test1.Remove(entity);
            }
        };

        go.Activate();

        ref var pooledObjectView = ref stashDisposable.Add(entity);
        pooledObjectView.go = go;
        this.world.Commit();

        Assert.True(this.test1.Has(entity));
        Assert.True(stashDisposable.Has(entity));
        
        // Remove entity, which triggers Dispose() and the OnDeactivate callback
        this.world.RemoveEntity(entity);

        Assert.True(this.world.IsDisposed(entity));

        this.world.Commit();

        var newEntity1 = this.world.CreateEntity();
        this.test1.Add(newEntity1);
        Assert.NotEqual(newEntity1, entity);
        // A double removal would corrupt the generation, making below asserts fail
        Assert.Equal(entity.Generation + 1, newEntity1.Generation);
        this.world.Commit();
        
        var newEntity2 = this.world.CreateEntity();
        Assert.NotEqual(newEntity2, newEntity1);
        Assert.NotEqual(newEntity2, entity);
        this.test1.Add(newEntity2);
        this.world.Commit();
    }
}