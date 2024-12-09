using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class FilterMatchTests {
    private readonly ITestOutputHelper output;
    private readonly World world;
    
    public FilterMatchTests(ITestOutputHelper output) {
        this.output = output;
        MLogger.SetInstance(new XUnitLogger(this.output));
        
        this.world = World.Create();
    }
    
    [Fact]
    public void SingleComponentDisposeMatches() {
        var filter = this.world.Filter.With<Test1>().Build();
        
        var entity = this.world.CreateEntity();
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test1>();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void SingleComponentAliveMatches() {
        var filter = this.world.Filter.With<Test1>().Build();
        var entity = this.world.CreateEntity();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        entity.AddComponent<Test2>();
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test1>();
        this.world.Commit();
        
        foreach (var _ in filter) { 
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void MultipleComponentsInstantMatchExactly() {
        var filter = this.world.Filter.With<Test1>().With<Test2>().Build();
        var entity = this.world.CreateEntity();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        entity.AddComponent<Test2>();
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test1>();
        entity.RemoveComponent<Test2>();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void MultipleComponentsGraduallyMatchExactly() {
        var filter = this.world.Filter.With<Test1>().With<Test2>().Build();
        var entity = this.world.CreateEntity();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test2>();
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test1>();
        this.world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test2>();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void MultipleComponentsMatchNonExact() {
        var filter = this.world.Filter.With<Test1>().With<Test2>().Build();
        var entity = this.world.CreateEntity();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test2>();
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.AddComponent<Test3>();
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test1>();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test2>();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void MissingComponentDoesntMatch() {
        var filter = this.world.Filter.With<Test1>().With<Test2>().Build();
        var entity = this.world.CreateEntity();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        this.world.Commit();
        
        Assert.Equal(1, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<Test1>()));
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test2>();
        this.world.Commit();

        Assert.Equal(1, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<Test1>().With<Test2>()));
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test2>();
        this.world.Commit();

        Assert.Equal(1, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<Test1>()));
        Assert.Equal(0, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<Test1>().With<Test2>()));
        filter.DumpFilterArchetypes(this.output);
        Assert.Equal(0, filter.GetLengthSlow());
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test2>();
        this.world.Commit();
        
        Assert.Equal(0, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<Test1>()));
        Assert.Equal(1, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<Test1>().With<Test2>()));
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test1>();
        this.world.Commit();
        
        Assert.Equal(0, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<Test1>()));
        Assert.Equal(0, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<Test1>().With<Test2>()));
        Assert.Equal(1, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<Test2>()));
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void FilterLateCreationMatches() {
        var entity = this.world.CreateEntity();
        entity.AddComponent<Test1>();
        this.world.Commit();
        
        var filter = this.world.Filter.With<Test1>().Build();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
    }
    
    [Fact]
    public void FilterSingleEntityDisposalRemovesMatch() {
        var entity = this.world.CreateEntity();
        entity.AddComponent<Test1>();
        this.world.Commit();
        
        entity.AddComponent<Test2>();
        this.world.Commit();
        
        var filter = this.world.Filter.With<Test1>().With<Test2>().Build();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.Dispose();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void FilterMultipleEntitiesDisposalRemovesMatch() {
        var entitiesCount = 8;
        var entities = new Entity[entitiesCount];
        
        for (var i = 0; i < entitiesCount; i++) {
            entities[i] = this.world.CreateEntity();
            entities[i].AddComponent<Test1>();
            entities[i].AddComponent<Test2>();
            
            this.world.Commit();
            
            // We do this to ensure that there's no duplication in archetypes
            
            entities[i].RemoveComponent<Test1>();
            entities[i].RemoveComponent<Test2>();
            
            entities[i].AddComponent<Test1>();
            entities[i].AddComponent<Test2>();
            
            this.world.Commit();
        }
        
        var filter = this.world.Filter.With<Test1>().With<Test2>().Build();
        
        Assert.Equal(entitiesCount, filter.GetLengthSlow());
        var index = entities.Length;
        foreach (var filterEntity in filter) {
            Assert.Equal(entities[--index], filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        for (var i = 0; i < entitiesCount; i++) {
            entities[i].Dispose();
            this.world.Commit();
        }
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void DisposeInsideFilterIterationWorks() {
        var filter = this.world.Filter.With<Test1>().With<Test2>().Build();
        
        for (var i = 0; i < 8; i++) {
            var entity = this.world.CreateEntity();
            entity.AddComponent<Test1>();
            entity.AddComponent<Test2>();
        }
        
        this.world.Commit();
        
        Assert.Equal(8, filter.GetLengthSlow());
        foreach (var entity in filter) {
            entity.Dispose();
            Assert.True(this.world.IsDisposed(entity));
        }
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void RemoveLastComponentInsideIterationWorks() {
        var filter = this.world.Filter.With<Test1>().With<Test2>().Build();
        
        for (var i = 0; i < 8; i++) {
            var entity = this.world.CreateEntity();
            entity.AddComponent<Test1>();
            entity.AddComponent<Test2>();
        }
        
        this.world.Commit();
        
        Assert.Equal(8, filter.GetLengthSlow());
        foreach (var entity in filter) {
            entity.RemoveComponent<Test1>();
            entity.RemoveComponent<Test2>();
            Assert.False(this.world.IsDisposed(entity));
        }
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void AddRemoveSameComponentInsideIterationWorks() {
        var filter = this.world.Filter.With<Test1>().With<Test2>().Build();
        
        for (var i = 0; i < 8; i++) {
            var entity = this.world.CreateEntity();
            entity.AddComponent<Test1>();
            entity.AddComponent<Test2>();
        }
        
        this.world.Commit();
        Assert.Equal(8, filter.GetLengthSlow());
        
        foreach (var entity in filter) {
            entity.RemoveComponent<Test1>();
            entity.AddComponent<Test1>();
            Assert.False(this.world.IsDisposed(entity));
        }
        
        this.world.Commit();
        Assert.Equal(8, filter.GetLengthSlow());
    }
    
    [Fact]
    public void RemoveAddSameComponentInsideIterationWorks() {
        var filter = this.world.Filter.With<Test1>().With<Test2>().Build();
        var filterWithTest3 = this.world.Filter.With<Test1>().With<Test2>().With<Test3>().Build();
        
        for (var i = 0; i < 8; i++) {
            var entity = this.world.CreateEntity();
            entity.AddComponent<Test1>();
            entity.AddComponent<Test2>();
        }
        
        this.world.Commit();
        Assert.Equal(8, filter.GetLengthSlow());
        Assert.Equal(0, filterWithTest3.GetLengthSlow());
        
        foreach (var entity in filter) {
            entity.AddComponent<Test3>();
            entity.RemoveComponent<Test3>();
            Assert.False(this.world.IsDisposed(entity));
        }
        
        this.world.Commit();
        Assert.Equal(8, filter.GetLengthSlow());
        Assert.Equal(0, filterWithTest3.GetLengthSlow());
    }

    [Fact]
    public void LotsOfFiltersMatchCorrectly() {
        var f1 = this.world.Filter.With<Test1>().Build();
        var f2 = this.world.Filter.With<Test1>().Without<Test2>().Build();
        var f3 = this.world.Filter.With<Test1>().Without<Test2>().Without<Test3>().Build();
        var f4 = this.world.Filter.With<Test1>().Without<Test2>().Without<Test3>().Without<Test4>().Build();
        var f5 = this.world.Filter.With<Test1>().Without<Test2>().Without<Test3>().Without<Test4>().Without<Test5>().Build();
        var f6 = this.world.Filter.With<Test1>().Without<Test2>().Without<Test3>().Without<Test4>().Without<Test5>().Without<Test6>().Build();
        var f7 = this.world.Filter.With<Test1>().Without<Test2>().Without<Test3>().Without<Test4>().Without<Test5>().Without<Test6>().Without<Test7>().Build();
        var f8 = this.world.Filter.With<Test1>().Without<Test2>().Without<Test3>().Without<Test4>().Without<Test5>().Without<Test6>().Without<Test7>().Without<Test8>().Build();
        
        var entity = this.world.CreateEntity();
        entity.AddComponent<Test1>();
        
        this.world.Commit();
        
        Assert.Equal(1, f1.GetLengthSlow());
        Assert.Equal(1, f2.GetLengthSlow());
        Assert.Equal(1, f3.GetLengthSlow());
        Assert.Equal(1, f4.GetLengthSlow());
        Assert.Equal(1, f5.GetLengthSlow());
        Assert.Equal(1, f6.GetLengthSlow());
        Assert.Equal(1, f7.GetLengthSlow());
        Assert.Equal(1, f8.GetLengthSlow());
        
        entity.RemoveComponent<Test1>();
        this.world.Commit();
        
        Assert.Equal(0, f1.GetLengthSlow());
        Assert.Equal(0, f2.GetLengthSlow());
        Assert.Equal(0, f3.GetLengthSlow());
        Assert.Equal(0, f4.GetLengthSlow());
        Assert.Equal(0, f5.GetLengthSlow());
        Assert.Equal(0, f6.GetLengthSlow());
        Assert.Equal(0, f7.GetLengthSlow());
        Assert.Equal(0, f8.GetLengthSlow());
    }
}