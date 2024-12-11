using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

using Scellecs.Morpeh;

[Collection("Sequential")]
public class ArchetypeStoreTests(ITestOutputHelper output) {
    private readonly ITestOutputHelper output = output;

    [Fact]
    public void AddArchetype_AddsArchetypeToStore() {
        var archetypeStore = new ArchetypeStore();

        var archetype = new Archetype(default(ArchetypeHash).With<Test1>());
        archetypeStore.Add(archetype);

        Assert.True(archetypeStore.TryGet(archetype.hash, out var storedArchetype));
        Assert.Equal(archetype, storedArchetype);
    }
    
    [Fact]
    public void RemoveArchetype_RemovesArchetypeFromStore() {
        var archetypeStore = new ArchetypeStore();

        var archetype = new Archetype(default(ArchetypeHash).With<Test1>());
        archetypeStore.Add(archetype);

        Assert.True(archetypeStore.TryGet(archetype.hash, out _));
        
        archetypeStore.Remove(archetype);
        
        Assert.False(archetypeStore.TryGet(archetype.hash, out _));
    }
    
    [Fact]
    public void Clear_WorksCorrectly() {
        var archetypeStore = new ArchetypeStore();

        var archetype1 = new Archetype(default(ArchetypeHash).With<Test1>());
        var archetype2 = new Archetype(default(ArchetypeHash).With<Test2>());
        var archetype3 = new Archetype(default(ArchetypeHash).With<Test3>());
        
        archetypeStore.Add(archetype1);
        archetypeStore.Add(archetype2);
        archetypeStore.Add(archetype3);

        Assert.True(archetypeStore.TryGet(archetype1.hash, out var a1));
        Assert.Equal(archetype1, a1);
        
        Assert.True(archetypeStore.TryGet(archetype2.hash, out var a2));
        Assert.Equal(archetype2, a2);
        
        Assert.True(archetypeStore.TryGet(archetype3.hash, out var a3));
        Assert.Equal(archetype3, a3);
        
        archetypeStore.Clear();
        
        Assert.False(archetypeStore.TryGet(archetype1.hash, out _));
        Assert.False(archetypeStore.TryGet(archetype2.hash, out _));
        Assert.False(archetypeStore.TryGet(archetype3.hash, out _));
    }
    
    [Fact]
    public void TryGet_ReturnsFalseForNonExistentArchetype() {
        var archetypeStore = new ArchetypeStore();

        Assert.False(archetypeStore.TryGet(default, out _));
    }
    
    [Fact]
    public void Enumerator_Works() {
        var archetypeStore = new ArchetypeStore();

        var archetype1 = new Archetype(default(ArchetypeHash).With<Test1>());
        var archetype2 = new Archetype(default(ArchetypeHash).With<Test2>());
        var archetype3 = new Archetype(default(ArchetypeHash).With<Test3>());
        
        archetypeStore.Add(archetype1);
        archetypeStore.Add(archetype2);
        archetypeStore.Add(archetype3);

        archetypeStore.Remove(archetype2);

        var results = new List<Archetype>();
        foreach (var archetype in archetypeStore) {
            results.Add(archetype);
        }

        Assert.Equal(2, results.Count);
        Assert.Contains(archetype1, results);
        Assert.Contains(archetype3, results);
        Assert.DoesNotContain(archetype2, results);
    }
}