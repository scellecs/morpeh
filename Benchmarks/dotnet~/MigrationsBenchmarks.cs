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
    
    private Stash<IntTest1> intTest1;
    private Stash<IntTest2> intTest2;
    private Stash<IntTest3> intTest3;
    
    private TagStash tagTest2;
    private TagStash tagTest3;
    
    private Filter filter;
    
    private Entity[] buffer;
    
    [GlobalSetup]
    public void SetUp() {
        this.world = World.Create();
        
        this.intTest1 = IntTest1.GetStash(this.world);
        this.intTest2 = IntTest2.GetStash(this.world);
        this.intTest3 = IntTest3.GetStash(this.world);
        
        this.tagTest2 = TagTest2.GetStash(this.world);
        this.tagTest3 = TagTest3.GetStash(this.world);
        
        this.filter = this.world.Filter.With<IntTest1>().Build();
        
        this.buffer = new Entity[Count];
        for (var i = 0; i < Count; i++) {
            this.buffer[i] = this.world.CreateEntity();
            this.intTest1.Set(this.buffer[i], new IntTest1());
        }
        
        this.world.Commit();
    }

    // [Benchmark]
    public void DataStash_SetRemoveComponentArraySingle() {
        foreach (var entity in this.buffer) {
            this.intTest2.Set(entity, new IntTest2());
        }
        
        this.world.Commit();
        
        foreach (var entity in this.buffer) {
            this.intTest2.Remove(entity);
        }
        
        this.world.Commit();
    }
    
    // [Benchmark]
    public void TagStash_SetRemoveComponentArraySingle() {
        foreach (var entity in this.buffer) {
            this.tagTest2.Set(entity);
        }
        
        this.world.Commit();
        
        foreach (var entity in this.buffer) {
            this.tagTest2.Remove(entity);
        }
        
        this.world.Commit();
    }
    
    // [Benchmark]
    public void DataStash_SetRemoveComponentFilterSingle() {
        foreach (var entity in this.filter) {
            this.intTest2.Set(entity, new IntTest2());
        }
        
        this.world.Commit();
        
        foreach (var entity in this.filter) {
            this.intTest2.Remove(entity);
        }
        
        this.world.Commit();
    }
    
    // [Benchmark]
    public void TagStash_SetRemoveComponentFilterSingle() {
        foreach (var entity in this.filter) {
            this.tagTest2.Set(entity);
        }
        
        this.world.Commit();
        
        foreach (var entity in this.filter) {
            this.tagTest2.Remove(entity);
        }
        
        this.world.Commit();
    }
    
    // [Benchmark]
    public void DataStash_SetRemoveComponentFilterDouble() {
        foreach (var entity in this.filter) {
            this.intTest2.Set(entity, new IntTest2());
            this.intTest3.Set(entity, new IntTest3());
        }
        
        this.world.Commit();
        
        foreach (var entity in this.filter) {
            this.intTest2.Remove(entity);
            this.intTest3.Remove(entity);
        }
        
        this.world.Commit();
    }
    
    // [Benchmark]
    public void TagStash_SetRemoveComponentFilterDouble() {
        foreach (var entity in this.filter) {
            this.tagTest2.Set(entity);
            this.tagTest3.Set(entity);
        }
        
        this.world.Commit();
        
        foreach (var entity in this.filter) {
            this.tagTest2.Remove(entity);
            this.tagTest3.Remove(entity);
        }
        
        this.world.Commit();
    }
    
    // [Benchmark]
    public void NewArchetypeFuckery() {
        foreach (var entity in this.buffer) {
            this.intTest2.Set(entity, new IntTest2());
            this.world.Commit();
            this.intTest2.Remove(entity);
            this.world.Commit();
        }
    }
}