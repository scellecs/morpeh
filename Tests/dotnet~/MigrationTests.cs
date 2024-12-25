using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class MigrationTests {
    private readonly World world;
    
    public MigrationTests(ITestOutputHelper output) {
        world = World.Create();
        MLogger.SetInstance(new XUnitLogger(output));
    }
    
    [Fact]
    public void ArchetypeChanges() {
        var entity = this.world.CreateEntity();
        var previousArchetype = default(ArchetypeHash);
        
        entity.AddComponent<TagTest1>();
        
        // We haven't committed the changes yet, so the archetype should be the same
        Assert.Equal(previousArchetype, world.ArchetypeOf(entity));
        
        this.world.Commit();
        
        // Now that we've committed the changes, the archetype should have changed
        Assert.NotEqual(previousArchetype, world.ArchetypeOf(entity));
        var archetype = this.world.GetArchetype(default(ArchetypeHash).With<TagTest1>());
        Assert.Equal(1, archetype.length);
        
        entity.RemoveComponent<TagTest1>();
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
        
        entity.AddComponent<TagTest1>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>(), world.ArchetypeOf(entity));
        
        var archetypeT1 = this.world.GetArchetype(baseArchetype.With<TagTest1>());
        Assert.Equal(1, archetypeT1.length);
        
        entity.AddComponent<TagTest2>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        Assert.Equal(0, archetypeT1.length);
        
        var archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        entity.AddComponent<TagTest3>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2T3 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>());
        Assert.Equal(1, archetypeT1T2T3.length);
        Assert.Equal(0, archetypeT1T2.length);
        
        entity.AddComponent<TagTest4>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>().With<TagTest4>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2T3T4 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>().With<TagTest4>());
        Assert.Equal(1, archetypeT1T2T3T4.length);
        Assert.Equal(0, archetypeT1T2T3.length);
        
        entity.RemoveComponent<TagTest4>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>(), world.ArchetypeOf(entity));
        
        archetypeT1T2T3 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>());
        Assert.Equal(1, archetypeT1T2T3.length);
        Assert.Equal(0, archetypeT1T2T3T4.length);
        
        entity.RemoveComponent<TagTest3>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        Assert.Equal(0, archetypeT1T2T3.length);
        
        entity.RemoveComponent<TagTest2>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>(), world.ArchetypeOf(entity));
        
        archetypeT1 = this.world.GetArchetype(baseArchetype.With<TagTest1>());
        Assert.Equal(1, archetypeT1.length);
        Assert.Equal(0, archetypeT1T2.length);
        
        entity.RemoveComponent<TagTest1>();
        this.world.Commit();
        Assert.Equal(baseArchetype, world.ArchetypeOf(entity));
        
        Assert.Equal(0, archetypeT1.length);
    }
    
    [Fact]
    public void MultipleComponentsMigrate() {
        var entity = this.world.CreateEntity();
        var baseArchetype = world.ArchetypeOf(entity);
        
        entity.AddComponent<TagTest1>();
        entity.AddComponent<TagTest2>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        entity.AddComponent<TagTest3>();
        entity.AddComponent<TagTest4>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>().With<TagTest4>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2T3T4 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>().With<TagTest3>().With<TagTest4>());
        Assert.Equal(1, archetypeT1T2T3T4.length);
        Assert.Equal(0, archetypeT1T2.length);
        
        entity.RemoveComponent<TagTest4>();
        entity.RemoveComponent<TagTest3>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        Assert.Equal(0, archetypeT1T2T3T4.length);
        
        entity.RemoveComponent<TagTest2>();
        entity.RemoveComponent<TagTest1>();
        this.world.Commit();
        Assert.Equal(baseArchetype, world.ArchetypeOf(entity));
        
        Assert.Equal(0, archetypeT1T2.length);
    }
    
    [Fact]
    public void RemoveSameComponentDoesntMigrate() {
        var entity = this.world.CreateEntity();
        var baseArchetype = world.ArchetypeOf(entity);
        
        entity.AddComponent<TagTest1>();
        entity.AddComponent<TagTest2>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        entity.AddComponent<TagTest3>();
        entity.RemoveComponent<TagTest3>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        Assert.Equal(1, archetypeT1T2.length);
    }
    
    [Fact]
    public void NoStructuralChangeDoesntMigrate() {
        var entity = this.world.CreateEntity();
        var baseArchetype = world.ArchetypeOf(entity);
        
        entity.AddComponent<TagTest1>();
        entity.AddComponent<TagTest2>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        entity.RemoveComponent<TagTest3>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        Assert.Equal(1, archetypeT1T2.length);
    }
    
    [Fact]
    public void DisposeMigrates() {
        var entity = world.CreateEntity();
        var baseArchetype = default(ArchetypeHash);
        
        entity.AddComponent<TagTest1>();
        entity.AddComponent<TagTest2>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<TagTest1>().With<TagTest2>(), world.ArchetypeOf(entity));
        
        var archetypeT1T2 = world.GetArchetype(baseArchetype.With<TagTest1>().With<TagTest2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        entity.Dispose();
        this.world.Commit();
        Assert.Equal(baseArchetype, world.ArchetypeOf(entity));
        
        Assert.Equal(0, archetypeT1T2.length);
    }
}