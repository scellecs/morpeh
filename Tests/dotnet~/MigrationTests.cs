using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class MigrationTests {
    private readonly World world;

    private readonly TagStash tagTest1;
    private readonly TagStash tagTest2;
    private readonly TagStash tagTest3;
    private readonly TagStash tagTest4;
    
    public MigrationTests(ITestOutputHelper output) {
        MLogger.SetInstance(new XUnitLogger(output));
        
        this.world = World.Create();

        this.tagTest1 = TagTest1.GetStash(this.world);
        this.tagTest2 = TagTest2.GetStash(this.world);
        this.tagTest3 = TagTest3.GetStash(this.world);
        this.tagTest4 = TagTest4.GetStash(this.world);
    }
    
    [Fact]
    public void ArchetypeChanges() {
        var entity = this.world.CreateEntity();
        var previousArchetype = default(ArchetypeHash);
        
        this.tagTest1.Set(entity);
        
        // We haven't committed the changes yet, so the archetype should be the same
        Assert.Equal(previousArchetype, world.ArchetypeOf(entity));
        
        this.world.Commit();
        
        // Now that we've committed the changes, the archetype should have changed
        Assert.NotEqual(previousArchetype, world.ArchetypeOf(entity));
        var archetype = this.world.GetArchetype(default(ArchetypeHash).With<TagTest1>());
        Assert.Equal(1, archetype.length);
        
        this.tagTest1.Remove(entity);
        this.world.Commit();
        
        Assert.Equal(0, archetype.length);
        
        // The archetype should have changed again back to the original
        Assert.Equal(previousArchetype, world.ArchetypeOf(entity));
    }
    
    [Fact]
    public void SingleComponentMigrates() {
        var entity = this.world.CreateEntity();
        var baseArchetype = default(ArchetypeHash);
        
        Assert.Equal(baseArchetype, world.ArchetypeOf(entity));
        
        this.tagTest1.Set(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>(), world.ArchetypeOf(entity));
        
        var archetypeT1 = this.world.GetArchetype(baseArchetype.With<TagTest1>());
        Assert.Equal(1, archetypeT1.length);
        
        this.tagTest2.Set(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        Assert.Equal(0, archetypeT1.length);
        
        var archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        this.tagTest3.Set(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2T3 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>());
        Assert.Equal(1, archetypeT1T2T3.length);
        Assert.Equal(0, archetypeT1T2.length);
        
        this.tagTest4.Set(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>().With<TagTest4>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2T3T4 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>().With<TagTest4>());
        Assert.Equal(1, archetypeT1T2T3T4.length);
        Assert.Equal(0, archetypeT1T2T3.length);
        
        this.tagTest4.Remove(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>(), world.ArchetypeOf(entity));
        
        archetypeT1T2T3 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>());
        Assert.Equal(1, archetypeT1T2T3.length);
        Assert.Equal(0, archetypeT1T2T3T4.length);
        
        this.tagTest3.Remove(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        Assert.Equal(0, archetypeT1T2T3.length);
        
        this.tagTest2.Remove(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>(), world.ArchetypeOf(entity));
        
        archetypeT1 = this.world.GetArchetype(baseArchetype.With<TagTest1>());
        Assert.Equal(1, archetypeT1.length);
        Assert.Equal(0, archetypeT1T2.length);
        
        this.tagTest1.Remove(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype, world.ArchetypeOf(entity));
        
        Assert.Equal(0, archetypeT1.length);
    }
    
    [Fact]
    public void MultipleComponentsMigrate() {
        var entity = this.world.CreateEntity();
        var baseArchetype = world.ArchetypeOf(entity);
        
        this.tagTest1.Set(entity);
        this.tagTest2.Set(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        this.tagTest3.Set(entity);
        this.tagTest4.Set(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>().With<TagTest4>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2T3T4 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>().With<TagTest4>());
        Assert.Equal(1, archetypeT1T2T3T4.length);
        Assert.Equal(0, archetypeT1T2.length);
        
        this.tagTest4.Remove(entity);
        this.tagTest3.Remove(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        Assert.Equal(0, archetypeT1T2T3T4.length);
        
        this.tagTest2.Remove(entity);
        this.tagTest1.Remove(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype, world.ArchetypeOf(entity));
        
        Assert.Equal(0, archetypeT1T2.length);
    }
    
    [Fact]
    public void RemoveSameComponentDoesntMigrate() {
        var entity = this.world.CreateEntity();
        var baseArchetype = world.ArchetypeOf(entity);
        
        this.tagTest1.Set(entity);
        this.tagTest2.Set(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        this.tagTest3.Set(entity);
        this.tagTest3.Remove(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        Assert.Equal(1, archetypeT1T2.length);
    }
    
    [Fact]
    public void NoStructuralChangeDoesntMigrate() {
        var entity = this.world.CreateEntity();
        var baseArchetype = world.ArchetypeOf(entity);
        
        this.tagTest1.Set(entity);
        this.tagTest2.Set(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        this.tagTest3.Remove(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        Assert.Equal(1, archetypeT1T2.length);
    }
    
    [Fact]
    public void DisposeMigrates() {
        var entity = world.CreateEntity();
        var baseArchetype = default(ArchetypeHash);
        
        this.tagTest1.Set(entity);
        this.tagTest2.Set(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2 = world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        this.world.RemoveEntity(entity);
        this.world.Commit();
        Assert.Equal(baseArchetype, world.ArchetypeOf(entity));
        
        Assert.Equal(0, archetypeT1T2.length);
    }
}