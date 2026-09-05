using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class FilterMatchTests {
    private readonly ITestOutputHelper output;
    private readonly World world;
    
    private readonly TagStash tagTest1;
    private readonly TagStash tagTest2;
    private readonly TagStash tagTest3;
    
    public FilterMatchTests(ITestOutputHelper output) {
        this.output = output;
        MLogger.SetInstance(new XUnitLogger(this.output));
        
        this.world = World.Create();
        
        this.tagTest1 = TagTest1.GetStash(this.world);
        this.tagTest2 = TagTest2.GetStash(this.world);
        this.tagTest3 = TagTest3.GetStash(this.world);
    }
    
    [Fact]
    public void SingleComponentDisposeMatches() {
        var filter = this.world.Filter.With<TagTest1>().Build();
        
        var entity = this.world.CreateEntity();
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        this.tagTest1.Set(entity);
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        this.tagTest1.Remove(entity);
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void SingleComponentAliveMatches() {
        var filter = this.world.Filter.With<TagTest1>().Build();
        var entity = this.world.CreateEntity();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        this.tagTest1.Set(entity);
        this.tagTest2.Set(entity);
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        this.tagTest1.Remove(entity);
        this.world.Commit();
        
        foreach (var _ in filter) { 
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void MultipleComponentsInstantMatchExactly() {
        var filter = this.world.Filter.With<TagTest1>().With<TagTest2>().Build();
        var entity = this.world.CreateEntity();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        this.tagTest1.Set(entity);
        this.tagTest2.Set(entity);
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        this.tagTest1.Remove(entity);
        this.tagTest2.Remove(entity);
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void MultipleComponentsGraduallyMatchExactly() {
        var filter = this.world.Filter.With<TagTest1>().With<TagTest2>().Build();
        var entity = this.world.CreateEntity();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        this.tagTest1.Set(entity);
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        this.tagTest2.Set(entity);
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        this.tagTest1.Remove(entity);
        this.world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        this.tagTest1.Set(entity);
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        this.tagTest2.Remove(entity);
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void MultipleComponentsMatchNonExact() {
        var filter = this.world.Filter.With<TagTest1>().With<TagTest2>().Build();
        var entity = this.world.CreateEntity();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        this.tagTest1.Set(entity);
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        this.tagTest2.Set(entity);
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        this.tagTest3.Set(entity);
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        this.tagTest1.Remove(entity);
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        this.tagTest1.Set(entity);
        this.world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        this.tagTest2.Remove(entity);
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void MissingComponentDoesntMatch() {
        var filter = this.world.Filter.With<TagTest1>().With<TagTest2>().Build();
        var entity = this.world.CreateEntity();
        this.world.Commit();
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        this.tagTest1.Set(entity);
        this.world.Commit();
        
        Assert.Equal(1, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<TagTest1>()));
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        this.tagTest2.Set(entity);
        this.world.Commit();

        Assert.Equal(1, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<TagTest1>().With<TagTest2>()));
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        this.tagTest2.Remove(entity);
        this.world.Commit();

        Assert.Equal(1, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<TagTest1>()));
        Assert.Equal(0, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<TagTest1>().With<TagTest2>()));
        filter.DumpFilterArchetypes(this.output);
        Assert.Equal(0, filter.GetLengthSlow());
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        this.tagTest2.Set(entity);
        this.world.Commit();
        
        Assert.Equal(0, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<TagTest1>()));
        Assert.Equal(1, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<TagTest1>().With<TagTest2>()));
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        this.tagTest1.Remove(entity);
        this.world.Commit();
        
        Assert.Equal(0, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<TagTest1>()));
        Assert.Equal(0, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<TagTest1>().With<TagTest2>()));
        Assert.Equal(1, this.world.ArchetypeLengthOf(default(ArchetypeHash).With<TagTest2>()));
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void FilterLateCreationMatches() {
        var entity = this.world.CreateEntity();
        this.tagTest1.Set(entity);
        this.world.Commit();
        
        var filter = this.world.Filter.With<TagTest1>().Build();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
    }
    
    [Fact]
    public void FilterSingleEntityDisposalRemovesMatch() {
        var entity = this.world.CreateEntity();
        this.tagTest1.Set(entity);
        this.world.Commit();
        
        this.tagTest2.Set(entity);
        this.world.Commit();
        
        var filter = this.world.Filter.With<TagTest1>().With<TagTest2>().Build();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter) {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        this.world.RemoveEntity(entity);
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
            this.tagTest1.Set(entities[i]);
            this.tagTest2.Set(entities[i]);
            
            this.world.Commit();
            
            // We do this to ensure that there's no duplication in archetypes
            
            this.tagTest1.Remove(entities[i]);
            this.tagTest2.Remove(entities[i]);
            
            this.tagTest1.Set(entities[i]);
            this.tagTest2.Set(entities[i]);
            
            this.world.Commit();
        }
        
        var filter = this.world.Filter.With<TagTest1>().With<TagTest2>().Build();
        
        Assert.Equal(entitiesCount, filter.GetLengthSlow());
        var index = entities.Length;
        foreach (var filterEntity in filter) {
            Assert.Equal(entities[--index], filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        for (var i = 0; i < entitiesCount; i++) {
            this.world.RemoveEntity(entities[i]);
            this.world.Commit();
        }
        
        foreach (var _ in filter) {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void DisposeInsideFilterIterationWorks() {
        var filter = this.world.Filter.With<TagTest1>().With<TagTest2>().Build();
        
        for (var i = 0; i < 8; i++) {
            var entity = this.world.CreateEntity();
            this.tagTest1.Set(entity);
            this.tagTest2.Set(entity);
        }
        
        this.world.Commit();
        
        Assert.Equal(8, filter.GetLengthSlow());
        foreach (var entity in filter) {
            this.world.RemoveEntity(entity);
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
        var filter = this.world.Filter.With<TagTest1>().With<TagTest2>().Build();
        
        for (var i = 0; i < 8; i++) {
            var entity = this.world.CreateEntity();
            this.tagTest1.Set(entity);
            this.tagTest2.Set(entity);
        }
        
        this.world.Commit();
        
        Assert.Equal(8, filter.GetLengthSlow());
        foreach (var entity in filter) {
            this.tagTest1.Remove(entity);
            this.tagTest2.Remove(entity);
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
        var filter = this.world.Filter.With<TagTest1>().With<TagTest2>().Build();
        
        for (var i = 0; i < 8; i++) {
            var entity = this.world.CreateEntity();
            this.tagTest1.Set(entity);
            this.tagTest2.Set(entity);
        }
        
        this.world.Commit();
        Assert.Equal(8, filter.GetLengthSlow());
        
        foreach (var entity in filter) {
            this.tagTest1.Remove(entity);
            this.tagTest1.Set(entity);
            Assert.False(this.world.IsDisposed(entity));
        }
        
        this.world.Commit();
        Assert.Equal(8, filter.GetLengthSlow());
    }
    
    [Fact]
    public void RemoveAddSameComponentInsideIterationWorks() {
        var filter = this.world.Filter.With<TagTest1>().With<TagTest2>().Build();
        var filterWithTest3 = this.world.Filter.With<TagTest1>().With<TagTest2>().With<TagTest3>().Build();
        
        for (var i = 0; i < 8; i++) {
            var entity = this.world.CreateEntity();
            this.tagTest1.Set(entity);
            this.tagTest2.Set(entity);
        }
        
        this.world.Commit();
        Assert.Equal(8, filter.GetLengthSlow());
        Assert.Equal(0, filterWithTest3.GetLengthSlow());
        
        foreach (var entity in filter) {
            this.tagTest3.Set(entity);
            this.tagTest3.Remove(entity);
            Assert.False(this.world.IsDisposed(entity));
        }
        
        this.world.Commit();
        Assert.Equal(8, filter.GetLengthSlow());
        Assert.Equal(0, filterWithTest3.GetLengthSlow());
    }

    [Fact]
    public void LotsOfFiltersMatchCorrectly() {
        var f1 = this.world.Filter.With<TagTest1>().Build();
        var f2 = this.world.Filter.With<TagTest1>().Without<TagTest2>().Build();
        var f3 = this.world.Filter.With<TagTest1>().Without<TagTest2>().Without<TagTest3>().Build();
        var f4 = this.world.Filter.With<TagTest1>().Without<TagTest2>().Without<TagTest3>().Without<TagTest4>().Build();
        var f5 = this.world.Filter.With<TagTest1>().Without<TagTest2>().Without<TagTest3>().Without<TagTest4>().Without<TagTest5>().Build();
        var f6 = this.world.Filter.With<TagTest1>().Without<TagTest2>().Without<TagTest3>().Without<TagTest4>().Without<TagTest5>().Without<TagTest6>().Build();
        var f7 = this.world.Filter.With<TagTest1>().Without<TagTest2>().Without<TagTest3>().Without<TagTest4>().Without<TagTest5>().Without<TagTest6>().Without<TagTest7>().Build();
        var f8 = this.world.Filter.With<TagTest1>().Without<TagTest2>().Without<TagTest3>().Without<TagTest4>().Without<TagTest5>().Without<TagTest6>().Without<TagTest7>().Without<TagTest8>().Build();
        
        var entity = this.world.CreateEntity();
        this.tagTest1.Set(entity);
        
        this.world.Commit();
        
        Assert.Equal(1, f1.GetLengthSlow());
        Assert.Equal(1, f2.GetLengthSlow());
        Assert.Equal(1, f3.GetLengthSlow());
        Assert.Equal(1, f4.GetLengthSlow());
        Assert.Equal(1, f5.GetLengthSlow());
        Assert.Equal(1, f6.GetLengthSlow());
        Assert.Equal(1, f7.GetLengthSlow());
        Assert.Equal(1, f8.GetLengthSlow());
        
        this.tagTest1.Remove(entity);
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