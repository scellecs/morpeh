using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class FilterMatchTests
{
    private readonly World world;
    
    public FilterMatchTests(ITestOutputHelper output)
    {
        world = World.Create();
        MLogger.SetInstance(new XUnitLogger(output));
    }
    
    [Fact]
    public void SingleComponentDisposeMatches()
    {
        var filter = world.Filter.With<Test1>().Build();
        
        var entity = world.CreateEntity();
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter)
        {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test1>();
        world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void SingleComponentAliveMatches()
    {
        var filter = world.Filter.With<Test1>().Build();
        
        var entity = world.CreateEntity();
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        entity.AddComponent<Test2>();
        world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter)
        {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test1>();
        world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void MultipleComponentsInstantMatchExactly()
    {
        var filter = world.Filter.With<Test1>().With<Test2>().Build();
        
        var entity = world.CreateEntity();
        world.Commit();
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        entity.AddComponent<Test2>();
        world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter)
        {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test1>();
        entity.RemoveComponent<Test2>();
        world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void MultipleComponentsGraduallyMatchExactly()
    {
        var filter = world.Filter.With<Test1>().With<Test2>().Build();
        
        var entity = world.CreateEntity();
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test2>();
        world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter)
        {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test1>();
        world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter)
        {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test2>();
        world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void MultipleComponentsMatchNonExact()
    {
        var filter = world.Filter.With<Test1>().With<Test2>().Build();
        
        var entity = world.CreateEntity();
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test2>();
        world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter)
        {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.AddComponent<Test3>();
        world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter)
        {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test1>();
        world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter)
        {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test2>();
        world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
    
    [Fact]
    public void MissingComponentDoesntMatch()
    {
        var filter = world.Filter.With<Test1>().With<Test2>().Build();
        
        var entity = world.CreateEntity();
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test1>();
        world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test2>();
        world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter)
        {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test2>();
        world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
        
        entity.AddComponent<Test2>();
        world.Commit();
        
        Assert.Equal(1, filter.GetLengthSlow());
        foreach (var filterEntity in filter)
        {
            Assert.Equal(entity, filterEntity);
        }
        Assert.Equal(1, filter.archetypesLength);
        
        entity.RemoveComponent<Test1>();
        world.Commit();
        
        foreach (var _ in filter)
        {
            Assert.Fail("Filter should be empty");
        }
        Assert.Equal(0, filter.archetypesLength);
    }
}