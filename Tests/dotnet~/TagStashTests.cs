using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class TagStashTests {
    private readonly World world;
    private readonly World world2;
    private readonly ITestOutputHelper output;

    public TagStashTests(ITestOutputHelper output) {
        this.world  = World.Create();
        this.world2 = World.Create();
        MLogger.SetInstance(new XUnitLogger(output));
        this.output = output;
    }

    [Fact]
    public void WorldGetStash_WorksCorretly() {
        var stash = TagTest1.GetStash(this.world);
        Assert.NotNull(stash);
        Assert.Equal(0, stash.Length);
        Assert.Equal(typeof(TagTest1), stash.Type);
        Assert.False(stash.IsDisposed);
        Assert.False(stash.IsNotEmpty());
        Assert.True(stash.IsEmpty());

        this.world.Commit();
    }

    [Fact]
    public void WorldGetReflectionStash_WorksCorretly() {
        var reflectionStash = this.world.GetReflectionStash(typeof(TagTest2));
        Assert.NotNull(reflectionStash);
        var stash = reflectionStash as TagStash;
        Assert.NotNull(stash);
        Assert.Equal(0, stash.Length);
        Assert.Equal(typeof(TagTest2), stash.Type);
        Assert.False(stash.IsDisposed);
        Assert.False(stash.IsNotEmpty());
        Assert.True(stash.IsEmpty());

        this.world.Commit();
    }

    [Fact]
    public void AddInComponent_AddsComponentSuccessfully() {
        var entity = this.world.CreateEntity();
        var stash = TagTest1.GetStash(this.world);
        var stashLength = stash.Length;
        stash.Add(entity);

        Assert.True(stash.Has(entity));
        Assert.Equal(stash.Length, stashLength + 1);

        this.world.Commit();
    }

    [Fact]
    public void AddSimple_ExistingThrowsException() {
        var entity = this.world.CreateEntity();
        var stash = TagTest2.GetStash(this.world);

        stash.Add(entity);
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity));

        this.world.Commit();
    }

    [Fact]
    public void AddInComponent_ExistingThrowsException() {
        var entity = this.world.CreateEntity();
        var stash = TagTest1.GetStash(this.world);

        stash.Add(entity);
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity));

        this.world.Commit();
    }

    [Fact]
    public void AddSameComponentWithDifferentOverloads_ThrowsException() {
        var entity = this.world.CreateEntity();
        var stash = TagTest2.GetStash(this.world);

        stash.Add(entity);
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity));

        this.world.Commit();
    }

    [Fact]
    public void SetSimple_SetsComponentSuccessfully() {
        var entity = this.world.CreateEntity();
        var stash = TagTest1.GetStash(this.world);
        var stashLength = stash.Length;
        stash.Add(entity);

        Assert.True(stash.Has(entity));
        Assert.Equal(stash.Length, stashLength + 1);

        this.world.Commit();
    }

    [Fact]
    public void Set_SetsComponentSuccessfully() {
        var entity = this.world.CreateEntity();
        var stash = TagTest1.GetStash(this.world);
        var stashLength = stash.Length;
        stash.Add(entity);

        Assert.True(stash.Has(entity));
        Assert.Equal(stash.Length, stashLength + 1);

        this.world.Commit();
    }

    [Fact]
    public void Migrate_MovesComponentBetweenEntities() {
        var fromEntity = this.world.CreateEntity();
        var toEntity = this.world.CreateEntity();
        var stash = TagTest1.GetStash(this.world);

        stash.Set(fromEntity);
        stash.Migrate(fromEntity, toEntity);

        Assert.False(stash.Has(fromEntity));
        Assert.True(stash.Has(toEntity));

        this.world.Commit();
    }

    [Fact]
    public void RemoveAll_ClearsAllComponents() {
        var entity1 = this.world.CreateEntity();
        var entity2 = this.world.CreateEntity();
        var stash = TagTest1.GetStash(this.world);

        stash.Set(entity1);
        stash.Set(entity2);

        stash.RemoveAll();

        Assert.Equal(0, stash.Length);
        Assert.True(stash.IsEmpty());
        Assert.False(stash.IsNotEmpty());

        this.world.Commit();
    }

    [Fact]
    public void Remove_NonExistentComponentDoesNotThrow() {
        var entity = this.world.CreateEntity();
        var stash = TagTest1.GetStash(this.world);

        var exception = Record.Exception(() => stash.Remove(entity));
        Assert.Null(exception);
        this.world.Commit();
    }

    [Fact]
    public void Remove_NonExistentComponentDoesNotDecreaseStashLength() {
        var entity = this.world.CreateEntity();
        var stash = TagTest1.GetStash(this.world);
        var stashLength = stash.Length;
        var removed = stash.Remove(entity);

        Assert.False(removed);
        Assert.Equal(stashLength, stash.Length);

        this.world.Commit();
    }

    [Fact]
    public void Remove_ExistentComponentDecreaseStashLength() {
        var entity = this.world.CreateEntity();
        var stash = TagTest1.GetStash(this.world);
        stash.Add(entity);
        var stashLength = stash.Length;
        var removed = stash.Remove(entity);

        Assert.True(removed);
        Assert.Equal(stashLength - 1, stash.Length);

        this.world.Commit();
    }

    [Fact]
    public void StashOperations_ThrowOnDisposedEntityWithoutWorldCommit() {
        var stash = TagTest1.GetStash(this.world);
        var stash2 = TagTest2.GetStash(this.world);
        var stash3 = TagTest4.GetStash(this.world);
        var entity = this.world.CreateEntity();
        var entity2 = this.world.CreateEntity();
        stash3.Add(entity);
        stash3.Add(entity2);
        this.world.RemoveEntity(entity);

        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash.Set(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash2.Set(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash2.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash2.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash2.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash3.Set(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash3.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash3.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash3.Migrate(entity, entity2));

        this.world.Commit();
    }

    [Fact]
    public void StashOperations_ThrowOnDisposedEntityWithWorldCommit() {
        var stash = TagTest1.GetStash(this.world);
        var stash2 = TagTest2.GetStash(this.world);
        var stash3 = TagTest4.GetStash(this.world);
        var entity = this.world.CreateEntity();
        var entity2 = this.world.CreateEntity();
        stash3.Add(entity);
        stash3.Add(entity2);
        this.world.RemoveEntity(entity);

        this.world.Commit();

        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash.Set(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash2.Set(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash2.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash2.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash2.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash3.Set(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash3.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash3.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash3.Migrate(entity, entity2));

        this.world.Commit();
    }

    [Fact]
    public void StashOperations_ThrowOnEntityFromDifferentWorld() {
        var stash = TagTest1.GetStash(this.world);
        var stash2 = TagTest2.GetStash(this.world);
        var stash3 = TagTest4.GetStash(this.world);
        var entity = this.world2.CreateEntity();
        var entity2 = this.world2.CreateEntity();

        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash.Set(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash2.Set(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash2.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash2.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash2.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash3.Set(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash3.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash3.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash3.Migrate(entity, entity2));

        this.world.Commit();
        this.world2.Commit();
    }

    [Fact]
    public void StashOperations_ThrowOnDisposedEntityFromDifferentWorld() {
        var stash = TagTest1.GetStash(this.world);
        var stash2 = TagTest2.GetStash(this.world);
        var stash3 = TagTest4.GetStash(this.world);
        var entity = this.world2.CreateEntity();
        var entity2 = this.world2.CreateEntity();
        this.world2.RemoveEntity(entity);

        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash.Set(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash2.Set(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash2.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash2.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash2.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash3.Set(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash3.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash3.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash3.Migrate(entity, entity2));

        this.world.Commit();
        this.world2.Commit();
    }

    [Fact]
    public void Stress_AddMultipleComponentsLargeNumberOfEntities() {
        var stash = TagTest1.GetStash(this.world);
        var stash2 = TagTest2.GetStash(this.world);
        var stash3 = TagTest3.GetStash(this.world);
        var stash4 = TagTest4.GetStash(this.world);
        var entities = new List<Entity>();
        const int entityCount = 10000;

        for (int i = 0; i < entityCount; i++) {
            var entity = this.world.CreateEntity();
            stash.Add(entity);
            stash2.Add(entity);
            stash3.Add(entity);
            stash4.Add(entity);
            entities.Add(entity);
        }

        Assert.Equal(entityCount, stash.Length);

        for (int i = 0; i < entityCount; i++) {
            Assert.True(stash.Has(entities[i]));
            Assert.True(stash2.Has(entities[i]));
            Assert.True(stash3.Has(entities[i]));
            Assert.True(stash4.Has(entities[i]));
        }

        this.world.Commit();
    }
}