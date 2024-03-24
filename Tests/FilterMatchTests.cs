using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class FilterMatchTests {
    private readonly ITestOutputHelper output;
    private readonly World world;
    
    public FilterMatchTests(ITestOutputHelper output) {
        this.output = output;
        this.world = World.Create();
        
        MLogger.SetInstance(new XUnitLogger(this.output));
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
        
        Assert.Equal(1, this.world.ArchetypeLengthOf(ArchetypeId.Invalid.With<Test1>()));
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test2>();
        this.world.Commit();

        Assert.Equal(1, this.world.ArchetypeLengthOf(ArchetypeId.Invalid.With<Test1>().With<Test2>()));
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test2>();
        this.world.Commit();

        Assert.Equal(1, this.world.ArchetypeLengthOf(ArchetypeId.Invalid.With<Test1>()));
        Assert.Equal(0, this.world.ArchetypeLengthOf(ArchetypeId.Invalid.With<Test1>().With<Test2>()));
        filter.DumpFilterArchetypes(this.output);
        Assert.Equal(0, filter.GetLengthSlow());
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test2>();
        this.world.Commit();
        
        Assert.Equal(0, this.world.ArchetypeLengthOf(ArchetypeId.Invalid.With<Test1>()));
        Assert.Equal(1, this.world.ArchetypeLengthOf(ArchetypeId.Invalid.With<Test1>().With<Test2>()));
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test1>();
        this.world.Commit();
        
        Assert.Equal(0, this.world.ArchetypeLengthOf(ArchetypeId.Invalid.With<Test1>()));
        Assert.Equal(0, this.world.ArchetypeLengthOf(ArchetypeId.Invalid.With<Test1>().With<Test2>()));
        Assert.Equal(1, this.world.ArchetypeLengthOf(ArchetypeId.Invalid.With<Test2>()));
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
        var index = 0;
        foreach (var filterEntity in filter) {
            Assert.Equal(entities[index++], filterEntity);
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
}