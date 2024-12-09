using Scellecs.Morpeh;
using Xunit.Abstractions;

namespace Tests;

[Collection("Sequential")]
public class FilterBuilderTests {
    private readonly ITestOutputHelper output;
    private readonly World world;
    
    public FilterBuilderTests(ITestOutputHelper output) {
        this.output = output;
        MLogger.SetInstance(new XUnitLogger(this.output));
        
        this.world = World.Create();
    }
    
    [Fact]
    public void SingleWithFilterBuilds() {
        var filter = this.world.Filter.With<Test1>().Build();
        
        Assert.Single(filter.includedTypeIds);
        Assert.Equal(ComponentId<Test1>.info.id, filter.includedTypeIds[0]);
        
        Assert.Empty(filter.excludedTypeIds);
    }
    
    [Fact]
    public void SingleWithoutFilterBuilds() {
        var filter = this.world.Filter.Without<Test1>().Build();
        
        Assert.Empty(filter.includedTypeIds);
        
        Assert.Single(filter.excludedTypeIds);
        Assert.Equal(ComponentId<Test1>.info.id, filter.excludedTypeIds[0]);
    }

    [Fact]
    public void CrossComponentReuseThrows() {
        var builder = this.world.Filter.With<Test1>().Without<Test2>();
        
        Assert.Throws<ComponentExistsInFilterException>(() => {
            builder.With<Test1>();
        });
        
        Assert.Throws<ComponentExistsInFilterException>(() => {
            builder.Without<Test1>();
        });
        
        Assert.Throws<ComponentExistsInFilterException>(() => {
            builder.With<Test2>();
        });
        
        Assert.Throws<ComponentExistsInFilterException>(() => {
            builder.Without<Test2>();
        });
    }
    
    [Fact]
    public void BuilderReuseAfterBuildThrows() {
        var builder = this.world.Filter.With<Test1>();
        builder.Build();
        
        Assert.Throws<FilterBuilderReuseException>(() => {
            builder.With<Test2>();
        });
    }
    
    [Fact]
    public void BuilderUnorderedReuseThrows() {
        var builder = this.world.Filter.With<Test1>();
        builder.With<Test2>();
        
        Assert.Throws<FilterBuilderReuseException>(() => {
            builder.With<Test3>();
        });
    }

    [Fact]
    public void BuilderCopyWorks() {
        var original = this.world.Filter.With<Test1>();
        var copy= original.Copy();

        var originalFilter = original.With<Test2>().Without<Test3>().Build();
        var copyFilter = copy.With<Test2>().Without<Test3>().Build();
        
        Assert.Same(originalFilter, copyFilter);
    }
}