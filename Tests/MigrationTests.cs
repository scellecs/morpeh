using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class MigrationTests
{
    private readonly World world;
    
    public MigrationTests(ITestOutputHelper output) {
        world = World.Create();
        MLogger.SetInstance(new XUnitLogger(output));
    }
    
    [Fact]
    public void ArchetypeChanges() {
        var entity = this.world.CreateEntity();
        var previousArchetype = ArchetypeId.Invalid;
        
        entity.AddComponent<Test1>();
        
        // We haven't committed the changes yet, so the archetype should be the same
        Assert.Equal(previousArchetype, entity.currentArchetype);
        
        this.world.Commit();
        
        // Now that we've committed the changes, the archetype should have changed
        Assert.NotEqual(previousArchetype, entity.currentArchetype);
        var archetype = this.world.GetArchetype(ArchetypeId.Invalid.With<Test1>());
        Assert.Equal(1, archetype.length);
        
        entity.RemoveComponent<Test1>();
        this.world.Commit();
        
        Assert.Equal(0, archetype.length);
        
        // The archetype should have changed again back to the original
        Assert.Equal(previousArchetype, entity.currentArchetype);
    }
    
    [Fact]
    public void SingleComponentMigrates() {
        var entity = this.world.CreateEntity();
        var baseArchetype = ArchetypeId.Invalid;
        
        Assert.Equal(baseArchetype, entity.currentArchetype);
        
        entity.AddComponent<Test1>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>(), entity.currentArchetype);
        
        var archetypeT1 = this.world.GetArchetype(baseArchetype.With<Test1>());
        Assert.Equal(1, archetypeT1.length);
        
        entity.AddComponent<Test2>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>().With<Test2>(), entity.currentArchetype);
        Assert.Equal(0, archetypeT1.length);
        
        var archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<Test1>().With<Test2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        entity.AddComponent<Test3>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>().With<Test2>().With<Test3>(), entity.currentArchetype);
        
        var archetypeT1T2T3 = this.world.GetArchetype(baseArchetype.With<Test1>().With<Test2>().With<Test3>());
        Assert.Equal(1, archetypeT1T2T3.length);
        Assert.Equal(0, archetypeT1T2.length);
        
        entity.AddComponent<Test4>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>().With<Test2>().With<Test3>().With<Test4>(), entity.currentArchetype);
        
        var archetypeT1T2T3T4 = this.world.GetArchetype(baseArchetype.With<Test1>().With<Test2>().With<Test3>().With<Test4>());
        Assert.Equal(1, archetypeT1T2T3T4.length);
        Assert.Equal(0, archetypeT1T2T3.length);
        
        entity.RemoveComponent<Test4>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>().With<Test2>().With<Test3>(), entity.currentArchetype);
        
        archetypeT1T2T3 = this.world.GetArchetype(baseArchetype.With<Test1>().With<Test2>().With<Test3>());
        Assert.Equal(1, archetypeT1T2T3.length);
        Assert.Equal(0, archetypeT1T2T3T4.length);
        
        entity.RemoveComponent<Test3>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>().With<Test2>(), entity.currentArchetype);
        
        archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<Test1>().With<Test2>());
        Assert.Equal(1, archetypeT1T2.length);
        Assert.Equal(0, archetypeT1T2T3.length);
        
        entity.RemoveComponent<Test2>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>(), entity.currentArchetype);
        
        archetypeT1 = this.world.GetArchetype(baseArchetype.With<Test1>());
        Assert.Equal(1, archetypeT1.length);
        Assert.Equal(0, archetypeT1T2.length);
        
        entity.RemoveComponent<Test1>();
        this.world.Commit();
        Assert.Equal(baseArchetype, entity.currentArchetype);
        
        Assert.Equal(0, archetypeT1.length);
    }
    
    [Fact]
    public void MultipleComponentsMigrate()
    {
        var entity = this.world.CreateEntity();
        var baseArchetype = entity.currentArchetype;
        
        entity.AddComponent<Test1>();
        entity.AddComponent<Test2>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>().With<Test2>(), entity.currentArchetype);
        
        var archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<Test1>().With<Test2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        entity.AddComponent<Test3>();
        entity.AddComponent<Test4>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>().With<Test2>().With<Test3>().With<Test4>(), entity.currentArchetype);
        
        var archetypeT1T2T3T4 = this.world.GetArchetype(baseArchetype.With<Test1>().With<Test2>().With<Test3>().With<Test4>());
        Assert.Equal(1, archetypeT1T2T3T4.length);
        Assert.Equal(0, archetypeT1T2.length);
        
        entity.RemoveComponent<Test4>();
        entity.RemoveComponent<Test3>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>().With<Test2>(), entity.currentArchetype);
        
        archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<Test1>().With<Test2>());
        Assert.Equal(1, archetypeT1T2.length);
        Assert.Equal(0, archetypeT1T2T3T4.length);
        
        entity.RemoveComponent<Test2>();
        entity.RemoveComponent<Test1>();
        this.world.Commit();
        Assert.Equal(baseArchetype, entity.currentArchetype);
        
        Assert.Equal(0, archetypeT1T2.length);
    }
    
    [Fact]
    public void RemoveSameComponentDoesntMigrate()
    {
        var entity = this.world.CreateEntity();
        var baseArchetype = entity.currentArchetype;
        
        entity.AddComponent<Test1>();
        entity.AddComponent<Test2>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>().With<Test2>(), entity.currentArchetype);
        
        var archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<Test1>().With<Test2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        entity.AddComponent<Test3>();
        entity.RemoveComponent<Test3>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>().With<Test2>(), entity.currentArchetype);
        
        Assert.Equal(1, archetypeT1T2.length);
    }
    
    [Fact]
    public void NoStructuralChangeDoesntMigrate()
    {
        var entity = this.world.CreateEntity();
        var baseArchetype = entity.currentArchetype;
        
        entity.AddComponent<Test1>();
        entity.AddComponent<Test2>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>().With<Test2>(), entity.currentArchetype);
        
        var archetypeT1T2 = this.world.GetArchetype(baseArchetype.With<Test1>().With<Test2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        entity.RemoveComponent<Test3>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>().With<Test2>(), entity.currentArchetype);
        
        Assert.Equal(1, archetypeT1T2.length);
    }
    
    [Fact]
    public void DisposeMigrates()
    {
        var entity = world.CreateEntity();
        var baseArchetype = ArchetypeId.Invalid;
        
        entity.AddComponent<Test1>();
        entity.AddComponent<Test2>();
        this.world.Commit();
        Assert.Equal(baseArchetype.With<Test1>().With<Test2>(), entity.currentArchetype);
        
        var archetypeT1T2 = world.GetArchetype(baseArchetype.With<Test1>().With<Test2>());
        Assert.Equal(1, archetypeT1T2.length);
        
        entity.Dispose();
        this.world.Commit();
        Assert.Equal(baseArchetype, entity.currentArchetype);
        
        Assert.Equal(0, archetypeT1T2.length);
    }
}