using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class StashTests {
    private struct StashSizeCheck : IComponent { }

    private readonly World world;
    private readonly World world2;
    private readonly ITestOutputHelper output;

    public StashTests(ITestOutputHelper output) {
        world = World.Create();
        world2 = World.Create();
        MLogger.SetInstance(new XUnitLogger(output));
        this.output = output;
    }

    [Fact]
    public void StashSize_WorksCorrectly() {
        ComponentId<StashSizeCheck>.StashSize = 512;

        var w1Stash = world.GetStash<StashSizeCheck>();
        var w2Stash = world2.GetStash<StashSizeCheck>();

        Assert.True(w1Stash.map.capacity >= 512);
        Assert.True(w2Stash.map.capacity >= 512);
    }

    [Fact]
    public void WorldGetStash_WorksCorretly() {
        var stash = this.world.GetStash<Test1>();
        Assert.NotNull(stash);
        Assert.Equal(0, stash.Length);
        Assert.Equal(typeof(Test1), stash.Type);
        Assert.False(stash.IsDisposed);
        Assert.False(stash.IsNotEmpty());
        Assert.True(stash.IsEmpty());

        this.world.Commit();
    }

    [Fact]
    public void WorldGetReflectionStash_WorksCorretly() {
        var reflectionStash = this.world.GetReflectionStash(typeof(Test2));
        Assert.NotNull(reflectionStash);
        var stash = reflectionStash as Stash<Test2>;
        Assert.NotNull(stash);
        Assert.Equal(0, stash.Length);
        Assert.Equal(typeof(Test2), stash.Type);
        Assert.False(stash.IsDisposed);
        Assert.False(stash.IsNotEmpty());
        Assert.True(stash.IsEmpty());

        this.world.Commit();
    }

    [Fact]
    public void AddSimple_AddsComponentSuccessfully() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();
        var stashLength = stash.Length;
        ref var component = ref stash.Add(entity);

        Assert.True(stash.Has(entity));
        Assert.Equal(stash.Length, stashLength + 1);

        component.value = 128;
        Assert.Equal(128, stash.Get(entity).value);

        this.world.Commit();
    }

    [Fact]
    public void AddOutExists_AddsComponentSuccessfully() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();
        var stashLength = stash.Length;
        ref var component = ref stash.Add(entity, out var exists);

        Assert.False(exists);
        Assert.True(stash.Has(entity));
        Assert.Equal(stash.Length, stashLength + 1);

        component.value = 512;
        Assert.Equal(512, stash.Get(entity).value);

        this.world.Commit();
    }

    [Fact]
    public void AddInComponent_AddsComponentSuccessfully() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<Test1>();
        var stashLength = stash.Length;
        stash.Add(entity, new Test1());
        ref var component = ref stash.Get(entity);

        Assert.True(stash.Has(entity));
        Assert.Equal(stash.Length, stashLength + 1);

        this.world.Commit();
    }

    [Fact]
    public void AddSimple_ExistingThrowsException() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<Test2>();

        stash.Add(entity);
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity));

        this.world.Commit();
    }

    [Fact]
    public void AddOutExists_ExistingDoesNotThrow() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<Test2>();

        stash.Add(entity, out var exists);
        Assert.False(exists);

        var exception = Record.Exception(() => {
            stash.Add(entity, out var existsAfter);
            Assert.True(existsAfter);
            output.WriteLine(existsAfter.ToString());
        });

        Assert.Null(exception);

        this.world.Commit();
    }

    [Fact]
    public void AddInComponent_ExistingThrowsException() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();

        stash.Add(entity, new IntTest1 { value = 42 });
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity, new IntTest1 { value = 43 }));

        this.world.Commit();
    }

    [Fact]
    public void AddSameComponentWithDifferentOverloads_ThrowsException() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<Test2>();

        stash.Add(entity);
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity, new Test2()));

        this.world.Commit();
    }

    [Fact]
    public void AddSimple_InitInplace() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();
        stash.Add(entity) = new IntTest1 { value = 587 };
        Assert.Equal(587, stash.Get(entity).value);

        this.world.Commit();
    }

    [Fact]
    public void AddOutExists_InitInplace() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();
        stash.Add(entity, out var exists) = new IntTest1 { value = 787 };
        Assert.False(exists);
        Assert.Equal(787, stash.Get(entity).value);

        this.world.Commit();
    }

    [Fact]
    public void SetSimple_SetsComponentSuccessfully() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<Test1>();
        var stashLength = stash.Length;
        stash.Add(entity);
        ref var component = ref stash.Get(entity);

        Assert.True(stash.Has(entity));
        Assert.Equal(stash.Length, stashLength + 1);

        this.world.Commit();
    }

    [Fact]
    public void Set_SetsComponentSuccessfully() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();
        var stashLength = stash.Length;
        stash.Add(entity, new IntTest1 { value = 42 });
        ref var component = ref stash.Get(entity);

        Assert.True(stash.Has(entity));
        Assert.Equal(42, component.value);
        Assert.Equal(stash.Length, stashLength + 1);

        this.world.Commit();
    }

    [Fact]
    public void Set_UpdatesExistingComponent() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();

        stash.Set(entity, new IntTest1 { value = 42 });
        stash.Set(entity, new IntTest1 { value = 100 });

        ref var component = ref stash.Get(entity);
        Assert.Equal(100, component.value);

        this.world.Commit();
    }

    [Fact]
    public void SetSimple_UpdatesExistingComponent() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();

        stash.Set(entity, new IntTest1 { value = 42 });
        stash.Set(entity);

        ref var component = ref stash.Get(entity);
        Assert.Equal(default, component);

        this.world.Commit();
    }

    [Fact]
    public void AddAndUpdateComponent_UpdatesSuccessfully() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();

        stash.Add(entity, new IntTest1 { value = 100 });
        stash.Set(entity, new IntTest1 { value = 200 });

        Assert.Equal(200, stash.Get(entity).value);

        this.world.Commit();
    }

    [Fact]
    public void Migrate_MovesComponentBetweenEntities() {
        var fromEntity = this.world.CreateEntity();
        var toEntity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();

        stash.Set(fromEntity, new IntTest1 { value = 42 });
        stash.Migrate(fromEntity, toEntity);

        Assert.False(stash.Has(fromEntity));
        Assert.True(stash.Has(toEntity));
        Assert.Equal(42, stash.Get(toEntity).value);

        this.world.Commit();
    }
    
    [Fact]
    public void Migrate_DoesNotOverwriteUnlessSpecified() {
        var fromEntity = this.world.CreateEntity();
        var toEntity   = this.world.CreateEntity();
        var stash      = this.world.GetStash<IntTest1>();

        stash.Set(fromEntity, new IntTest1 { value = 42 });
        stash.Set(toEntity, new IntTest1 { value = 100 });
        stash.Migrate(fromEntity, toEntity, overwrite: false);

        Assert.False(stash.Has(fromEntity));
        Assert.True(stash.Has(toEntity));
        Assert.Equal(100, stash.Get(toEntity).value);

        this.world.Commit();
    }
    
    [Fact]
    public void Migrate_OverwritesIfSpecified() {
        var fromEntity = this.world.CreateEntity();
        var toEntity   = this.world.CreateEntity();
        var stash      = this.world.GetStash<IntTest1>();

        stash.Set(fromEntity, new IntTest1 { value = 42 });
        stash.Set(toEntity, new IntTest1 { value   = 100 });
        stash.Migrate(fromEntity, toEntity, overwrite: true);

        Assert.False(stash.Has(fromEntity));
        Assert.True(stash.Has(toEntity));
        Assert.Equal(42, stash.Get(toEntity).value);

        this.world.Commit();
    }

    [Fact]
    public void RemoveAll_ClearsAllComponents() {
        var entity1 = this.world.CreateEntity();
        var entity2 = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();

        stash.Set(entity1, new IntTest1 { value = 42 });
        stash.Set(entity2, new IntTest1 { value = 100 });

        stash.RemoveAll();

        Assert.Equal(0, stash.Length);
        Assert.True(stash.IsEmpty());
        Assert.False(stash.IsNotEmpty());

        this.world.Commit();
    }

    [Fact]
    public void Remove_NonExistentComponentDoesNotThrow() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();

        var exception = Record.Exception(() => stash.Remove(entity));
        Assert.Null(exception);
        this.world.Commit();
    }

    [Fact]
    public void Remove_NonExistentComponentDoesNotDecreaseStashLength() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();
        var stashLength = stash.Length;
        var removed = stash.Remove(entity);

        Assert.False(removed);
        Assert.Equal(stashLength, stash.Length);

        this.world.Commit();
    }

    [Fact]
    public void Remove_ExistentComponentDecreaseStashLength() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<IntTest1>();
        stash.Add(entity);
        var stashLength = stash.Length;
        var removed = stash.Remove(entity);

        Assert.True(removed);
        Assert.Equal(stashLength - 1, stash.Length);

        this.world.Commit();
    }

    [Fact]
    public void Remove_ClearsData() {
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
    public void Remove_ClearsDataManaged() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<ManagedTest>();
        var valueManaged = new object();
        stash.Set(entity, new ManagedTest
        {
            value = valueManaged
        });

        Assert.NotNull(valueManaged);
        Assert.NotNull(stash.Get(entity).value);
        Assert.NotEqual(default, stash.Get(entity));

        ref var data = ref stash.Get(entity);
        Assert.Equal(valueManaged, data.value);

        stash.Remove(entity);

        Assert.False(stash.Has(entity));
        Assert.Equal(default, data.value);

        this.world.RemoveEntity(entity);
        this.world.Commit();
    }

    [Fact]
    public void EntityDispose_ClearsData() {
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

    [Fact]
    public void Enumerator_IteratesOverAllComponents() {
        var stash = this.world.GetStash<IntTest1>();
        var entities = new[] {
            this.world.CreateEntity(),
            this.world.CreateEntity(),
            this.world.CreateEntity(),
            this.world.CreateEntity(),
            this.world.CreateEntity(),
        };

        stash.Set(entities[0], new IntTest1 { value = 10 });
        stash.Set(entities[1], new IntTest1 { value = 20 });
        stash.Set(entities[2], new IntTest1 { value = 30 });
        stash.Set(entities[3], new IntTest1 { value = 40 });
        stash.Set(entities[4], new IntTest1 { value = 50 });

        int count = 0;
        int totalValue = 0;

        foreach (ref var component in stash) {
            count++;
            totalValue += component.value;
        }

        Assert.Equal(5, count);
        Assert.Equal(150, totalValue);
    }

    [Fact]
    public void Enumerator_ModifyComponentValues() {
        var stash = this.world.GetStash<IntTest1>();
        var entities = new[] {
            this.world.CreateEntity(),
            this.world.CreateEntity(),
            this.world.CreateEntity(),
            this.world.CreateEntity(),
            this.world.CreateEntity(),
        };

        stash.Set(entities[0], new IntTest1 { value = 10 });
        stash.Set(entities[1], new IntTest1 { value = 20 });
        stash.Set(entities[2], new IntTest1 { value = 30 });
        stash.Set(entities[3], new IntTest1 { value = 40 });
        stash.Set(entities[4], new IntTest1 { value = 50 });

        int count = 0;
        int totalValue = 0;

        foreach (ref var component in stash) {
            component.value = ++count;
        }

        for (int i = 0; i < entities.Length; i++) {
            var entity = entities[i];
            var component = stash.Get(entity);
            totalValue += component.value;
        }

        Assert.Equal(15, totalValue);
    }

    [Fact]
    public void EntityDispose_DisposableComponent() {
        var entity = this.world.CreateEntity();
        var stash = this.world.GetStash<DisposableTest>().AsDisposable();
        var handle = new DisposableTest.Handle();
        stash.Set(entity, new DisposableTest {
            value = handle,
        });

        Assert.True(handle.value);
        Assert.True(stash.Get(entity).value.value);
        Assert.Equal(handle, stash.Get(entity).value);

        ref var data = ref stash.Get(entity);
        this.world.RemoveEntity(entity);
        this.world.Commit();

        Assert.False(handle.value);
        Assert.Equal(default, data.value);
    }

    [Fact]
    public void WorldDispose_DisposableComponent() {
        var world = World.Create();
        world.Commit();

        var entity = world.CreateEntity();
        var stash = world.GetStash<DisposableTest>().AsDisposable();
        var handle = new DisposableTest.Handle();
        stash.Set(entity, new DisposableTest {
            value = handle,
        });

        Assert.True(handle.value);
        Assert.True(stash.Get(entity).value.value);
        Assert.Equal(handle, stash.Get(entity).value);

        ref var data = ref stash.Get(entity);
        world.Commit();
        world.Dispose();

        Assert.False(handle.value);
        Assert.Equal(default, data.value);
    }

    [Fact]
    public void StashOperations_ThrowOnDisposedEntityWithoutWorldCommit() {
        var stash = this.world.GetStash<Test1>();
        var stash2 = this.world.GetStash<Test2>();
        var stash3 = this.world.GetStash<Test4>();
        var entity = this.world.CreateEntity();
        var entity2 = this.world.CreateEntity();
        stash3.Add(entity);
        stash3.Add(entity2);
        this.world.RemoveEntity(entity);

        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity, out var exist));
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity, new Test1()));
        Assert.Throws<InvalidSetOperationException>(() => stash.Set(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash.Set(entity, new Test1()));
        Assert.Throws<InvalidGetOperationException>(() => stash.Get(entity, out var exists));
        Assert.Throws<InvalidGetOperationException>(() => stash.Get(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity, out var exist));
        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity, new Test2()));
        Assert.Throws<InvalidSetOperationException>(() => stash2.Set(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash2.Set(entity, new Test2()));
        Assert.Throws<InvalidGetOperationException>(() => stash2.Get(entity, out var exists));
        Assert.Throws<InvalidGetOperationException>(() => stash2.Get(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash2.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash2.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash2.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity, out var exist));
        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity, new Test4()));
        Assert.Throws<InvalidSetOperationException>(() => stash3.Set(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash3.Set(entity, new Test4()));
        Assert.Throws<InvalidGetOperationException>(() => stash3.Get(entity, out var exists));
        Assert.Throws<InvalidGetOperationException>(() => stash3.Get(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash3.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash3.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash3.Migrate(entity, entity2));

        this.world.Commit();
    }

    [Fact]
    public void StashOperations_ThrowOnDisposedEntityWithWorldCommit() {
        var stash = this.world.GetStash<Test1>();
        var stash2 = this.world.GetStash<Test2>();
        var stash3 = this.world.GetStash<Test4>();
        var entity = this.world.CreateEntity();
        var entity2 = this.world.CreateEntity();
        stash3.Add(entity);
        stash3.Add(entity2);
        this.world.RemoveEntity(entity);

        this.world.Commit();

        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity, out var exist));
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity, new Test1()));
        Assert.Throws<InvalidSetOperationException>(() => stash.Set(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash.Set(entity, new Test1()));
        Assert.Throws<InvalidGetOperationException>(() => stash.Get(entity, out var exists));
        Assert.Throws<InvalidGetOperationException>(() => stash.Get(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity, out var exist));
        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity, new Test2()));
        Assert.Throws<InvalidSetOperationException>(() => stash2.Set(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash2.Set(entity, new Test2()));
        Assert.Throws<InvalidGetOperationException>(() => stash2.Get(entity, out var exists));
        Assert.Throws<InvalidGetOperationException>(() => stash2.Get(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash2.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash2.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash2.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity, out var exist));
        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity, new Test4()));
        Assert.Throws<InvalidSetOperationException>(() => stash3.Set(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash3.Set(entity, new Test4()));
        Assert.Throws<InvalidGetOperationException>(() => stash3.Get(entity, out var exists));
        Assert.Throws<InvalidGetOperationException>(() => stash3.Get(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash3.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash3.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash3.Migrate(entity, entity2));

        this.world.Commit();
    }

    [Fact]
    public void StashOperations_ThrowOnEntityFromDifferentWorld() {
        var stash = this.world.GetStash<Test1>();
        var stash2 = this.world.GetStash<Test2>();
        var stash3 = this.world.GetStash<Test4>();
        var entity = this.world2.CreateEntity();
        var entity2 = this.world2.CreateEntity();

        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity, out var exist));
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity, new Test1()));
        Assert.Throws<InvalidSetOperationException>(() => stash.Set(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash.Set(entity, new Test1()));
        Assert.Throws<InvalidGetOperationException>(() => stash.Get(entity, out var exists));
        Assert.Throws<InvalidGetOperationException>(() => stash.Get(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity, out var exist));
        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity, new Test2()));
        Assert.Throws<InvalidSetOperationException>(() => stash2.Set(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash2.Set(entity, new Test2()));
        Assert.Throws<InvalidGetOperationException>(() => stash2.Get(entity, out var exists));
        Assert.Throws<InvalidGetOperationException>(() => stash2.Get(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash2.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash2.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash2.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity, out var exist));
        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity, new Test4()));
        Assert.Throws<InvalidSetOperationException>(() => stash3.Set(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash3.Set(entity, new Test4()));
        Assert.Throws<InvalidGetOperationException>(() => stash3.Get(entity, out var exists));
        Assert.Throws<InvalidGetOperationException>(() => stash3.Get(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash3.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash3.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash3.Migrate(entity, entity2));

        this.world.Commit();
        this.world2.Commit();
    }

    [Fact]
    public void StashOperations_ThrowOnDisposedEntityFromDifferentWorld() {
        var stash = this.world.GetStash<Test1>();
        var stash2 = this.world.GetStash<Test2>();
        var stash3 = this.world.GetStash<Test4>();
        var entity = this.world2.CreateEntity();
        var entity2 = this.world2.CreateEntity();
        this.world2.RemoveEntity(entity);

        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity, out var exist));
        Assert.Throws<InvalidAddOperationException>(() => stash.Add(entity, new Test1()));
        Assert.Throws<InvalidSetOperationException>(() => stash.Set(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash.Set(entity, new Test1()));
        Assert.Throws<InvalidGetOperationException>(() => stash.Get(entity, out var exists));
        Assert.Throws<InvalidGetOperationException>(() => stash.Get(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity, out var exist));
        Assert.Throws<InvalidAddOperationException>(() => stash2.Add(entity, new Test2()));
        Assert.Throws<InvalidSetOperationException>(() => stash2.Set(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash2.Set(entity, new Test2()));
        Assert.Throws<InvalidGetOperationException>(() => stash2.Get(entity, out var exists));
        Assert.Throws<InvalidGetOperationException>(() => stash2.Get(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash2.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash2.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash2.Migrate(entity, entity2));

        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity));
        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity, out var exist));
        Assert.Throws<InvalidAddOperationException>(() => stash3.Add(entity, new Test4()));
        Assert.Throws<InvalidSetOperationException>(() => stash3.Set(entity));
        Assert.Throws<InvalidSetOperationException>(() => stash3.Set(entity, new Test4()));
        Assert.Throws<InvalidGetOperationException>(() => stash3.Get(entity, out var exists));
        Assert.Throws<InvalidGetOperationException>(() => stash3.Get(entity));
        Assert.Throws<InvalidHasOperationException>(() => stash3.Has(entity));
        Assert.Throws<InvalidRemoveOperationException>(() => stash3.Remove(entity));
        Assert.Throws<InvalidMigrateOperationException>(() => stash3.Migrate(entity, entity2));

        this.world.Commit();
        this.world2.Commit();
    }

    [Fact]
    public void Stress_AddMultipleComponentsLargeNumberOfEntities() {
        var stash = this.world.GetStash<IntTest1>();
        var stash2 = this.world.GetStash<Test3>();
        var stash3 = this.world.GetStash<Test4>();
        var stash4 = this.world.GetStash<Test1>();
        var entities = new List<Entity>();
        const int entityCount = 10000;

        for (int i = 0; i < entityCount; i++) {
            var entity = this.world.CreateEntity();
            stash.Add(entity, new IntTest1 { value = i });
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
            Assert.Equal(i, stash.Get(entities[i]).value);
        }

        this.world.Commit();
    }

    [Theory]
    [InlineData(66)]
    [InlineData(82)]
    [InlineData(954376)]
    [InlineData(22)]
    public void Stress_StashOperations(int seed) {
        var stash = this.world.GetStash<IntTest1>();
        var tracker = new Dictionary<Entity, IntTest1>();
        var random = new Random(seed);
        const int operationsCount = 100000;

        for (int i = 0; i < operationsCount; i++) {
            var entity = this.world.CreateEntity();
            var operation = random.NextSingle();

            if (operation < 0.5f) {
                var component = new IntTest1 { value = random.Next() };
                
                var wasAddedToStash = !stash.Has(entity);
                if (wasAddedToStash) {
                    stash.Add(entity, component);
                }
                
                var wasAddedToTracker = !tracker.ContainsKey(entity);
                if (wasAddedToTracker) {
                    tracker[entity] = component;
                }

                Assert.Equal(wasAddedToTracker, wasAddedToStash);
            }
            else if (operation < 0.8f) {
                var wasRemovedFromStash = stash.Remove(entity);
                var wasRemovedFromTracker = tracker.Remove(entity);

                Assert.Equal(wasRemovedFromTracker, wasRemovedFromStash);
            }
            else {
                var stashComponent = stash.Get(entity, out var hasInStash);
                var hasInTracker = tracker.TryGetValue(entity, out var trackerComponent);

                Assert.Equal(hasInTracker, hasInStash);
                if (hasInTracker) {
                    Assert.Equal(trackerComponent, stashComponent);
                }
            }

            Assert.Equal(tracker.Count, stash.Length);

            if (i % 10000 == 0) {
                foreach (var kvp in tracker) {
                    var ent = kvp.Key;
                    var stashValue = stash.Get(ent, out var hasInStash);
                    Assert.True(hasInStash);
                    Assert.Equal(kvp.Value, stashValue);
                }
            }
        }

        Assert.Equal(tracker.Count, stash.Length);
        foreach (var kvp in tracker) {
            var ent = kvp.Key;
            var stashValue = stash.Get(ent, out var hasInStash);
            Assert.True(hasInStash);
            Assert.Equal(kvp.Value, stashValue);
        }
    }
}