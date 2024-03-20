using BenchmarkDotNet.Attributes;
using Scellecs.Morpeh.Collections;

namespace Benchmarks;

[MemoryDiagnoser(false)]
public class LongHashSetBenchmarks {
    private LongHashSet longHashSet;
    private HashSet<long> hashSet;
    
    [GlobalSetup]
    public void SetUp() {
        this.longHashSet = new LongHashSet(4);
        this.hashSet = new HashSet<long>(4);
    }
    
    //[Benchmark]
    public void LongHashSetAddRemove() {
        this.longHashSet.Add(77777721321231L);
        this.longHashSet.Remove(77777721321231L);
    }
    
    //[Benchmark]
    public void HashSetAddRemove() {
        this.hashSet.Add(77777721321231L);
        this.hashSet.Remove(77777721321231L);
    }
    
    //[Benchmark]
    public void LongHashSetAddHasRemove() {
        this.longHashSet.Add(77777721321231L);
        this.longHashSet.Has(77777721321231L);
        this.longHashSet.Remove(77777721321231L);
    }
    
    //[Benchmark]
    public void HashSetAddContainsRemove() {
        this.hashSet.Add(77777721321231L);
        this.hashSet.Contains(77777721321231L);
        this.hashSet.Remove(77777721321231L);
    }
}