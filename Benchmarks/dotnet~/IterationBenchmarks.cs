using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.dotTrace;
using Scellecs.Morpeh;
namespace Benchmarks;

public class IterationBenchmarks {
    [Params(1_000_000)]
    //[Params(16)]
    public int Count;

    private World world;
    
    private Stash<Test1> test1;
    private Stash<Test2> test2;
    private Stash<Test3> test3;
    
    private Filter filter;
    
    private Entity[] buffer;
    
    [GlobalSetup]
    public void SetUp() {
        this.world = World.Create();
        
        this.test1 = this.world.GetStash<Test1>();
        this.test2 = this.world.GetStash<Test2>();
        this.test3 = this.world.GetStash<Test3>();
        
        this.filter = this.world.Filter.With<Test1>().Build();
        
        this.buffer = new Entity[Count];
        for (var i = 0; i < Count; i++) {
            this.buffer[i] = this.world.CreateEntity();
            this.test1.Set(this.buffer[i], new Test1());
        }
        
        this.world.Commit();
    }
    
    // [Benchmark]
    public void ForeachFilterSingle() {
        foreach (var entity in this.filter) {
            this.test1.Get(entity).value++;
        }
    }
    
    // [Benchmark]
    public void ForeachFilterSingleManual() {
        for (var archetype = this.filter.archetypesLength - 1; archetype >= 0; archetype--) {
            var arch = this.filter.archetypes[archetype];
            var entities = arch.entities;
            
            for (var i = arch.length - 1; i >= 0; i--) {
                this.test1.Get(entities[i]).value++;
            }
        }
    }
}