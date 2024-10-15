using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.dotTrace;
using Scellecs.Morpeh;

namespace Benchmarks;

[MemoryDiagnoser(false)]
// [DotTraceDiagnoser]
// [HardwareCounters(HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions)]
public class MigrationsBenchmarks {
    [Params(16, 512, 2048)]
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
    public void SetRemoveComponentArraySingle() {
        foreach (var entity in this.buffer) {
            this.test2.Set(entity, new Test2());
        }
        
        this.world.Commit();
        
        foreach (var entity in this.buffer) {
            this.test2.Remove(entity);
        }
        
        this.world.Commit();
    }
    
    // [Benchmark]
    public void SetRemoveComponentFilterSingle() {
        foreach (var entity in this.filter) {
            this.test2.Set(entity, new Test2());
        }
        
        this.world.Commit();
        
        foreach (var entity in this.filter) {
            this.test2.Remove(entity);
        }
        
        this.world.Commit();
    }
    
    // [Benchmark]
    public void SetRemoveComponentFilterDouble() {
        foreach (var entity in this.filter) {
            this.test2.Set(entity, new Test2());
            this.test3.Set(entity, new Test3());
        }
        
        this.world.Commit();
        
        foreach (var entity in this.filter) {
            this.test2.Remove(entity);
            this.test3.Remove(entity);
        }
        
        this.world.Commit();
    }
    
    // [Benchmark]
    public void NewArchetypeFuckery() {
        foreach (var entity in this.buffer) {
            this.test2.Set(entity, new Test2());
            this.world.Commit();
            this.test2.Remove(entity);
            this.world.Commit();
        }
    }
}