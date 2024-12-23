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
    
    private Stash<IntTest1> intTest1;
    
    private Filter filter;
    
    private Entity[] buffer;
    
    [GlobalSetup]
    public void SetUp() {
        this.world = World.Create();

        this.intTest1 = IntTest1.GetStash(this.world);
        
        this.filter = this.world.Filter.With<IntTest1>().Build();
        
        this.buffer = new Entity[Count];
        for (var i = 0; i < Count; i++) {
            this.buffer[i] = this.world.CreateEntity();
            this.intTest1.Set(this.buffer[i], new IntTest1());
        }
        
        this.world.Commit();
    }
    
    // [Benchmark]
    public void ForeachFilterSingle() {
        foreach (var entity in this.filter) {
            this.intTest1.Get(entity).value++;
        }
    }
    
    // [Benchmark]
    public void ForeachFilterSingleManual() {
        for (var archetype = this.filter.archetypesLength - 1; archetype >= 0; archetype--) {
            var arch = this.filter.archetypes[archetype];
            var entities = arch.entities;
            
            for (var i = arch.length - 1; i >= 0; i--) {
                this.intTest1.Get(entities[i]).value++;
            }
        }
    }
}