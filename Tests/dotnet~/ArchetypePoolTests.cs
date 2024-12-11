using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

using Scellecs.Morpeh;

[Collection("Sequential")]
public class ArchetypePoolTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void ConstructorWarmsUpInitialCapacity() {
        var pool = new ArchetypePool(44);
        
        Assert.Equal(44, pool.archetypes.Length);
        Assert.Equal(44, pool.count);
    }
    
    [Fact]
    public void ManualWarmUpAddsToCapacityAndCount() {
        var pool = new ArchetypePool(44);
        
        pool.Rent(default); // 43
        pool.Rent(default); // 42
        
        Assert.Equal(44, pool.archetypes.Length);
        Assert.Equal(42, pool.count);
        
        pool.WarmUp(10);
        
        Assert.Equal(52, pool.archetypes.Length);
        Assert.Equal(52, pool.count);
    }
    
    [Fact]
    public void RentReturnWorksCorrectlyWithoutCreation() {
        var pool = new ArchetypePool(44);
        
        var lastArchetype = pool.archetypes[^1];
        var archetype = pool.Rent(new ArchetypeHash(123));
        Assert.Equal(lastArchetype, archetype);
        
        Assert.Equal(43, pool.count);
        
        pool.Return(archetype);
        Assert.Equal(44, pool.count);
        Assert.Equal(archetype, pool.archetypes[^1]);
    }
    
    [Fact]
    public void RentReturnCreatesNewArchetype() {
        var pool = new ArchetypePool(1);
        pool.Rent(new ArchetypeHash(123));
        
        Assert.Equal(0, pool.count);
        
        var archetype = pool.Rent(new ArchetypeHash(456));
        Assert.Equal(0, pool.count);
        
        pool.Return(archetype);
        Assert.Equal(1, pool.count);
        Assert.Equal(archetype, pool.archetypes[0]);
    }
}