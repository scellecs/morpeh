using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.dotTrace;
using Scellecs.Morpeh;
namespace Benchmarks;

using System.Runtime.InteropServices;

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
    
    [Benchmark]
    public void ForeachFilterSingle() {
        ref var test1Ref = ref this.test1.Ref();
        foreach (var entity in this.filter) {
            this.test1.Get(entity, ref test1Ref).value++;
        }
    }
}