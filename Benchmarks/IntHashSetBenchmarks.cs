using BenchmarkDotNet.Attributes;
using Scellecs.Morpeh.Collections;

namespace Benchmarks;

[MemoryDiagnoser(false)]
public class IntHashSetBenchmarks
{
    private readonly HashSet<int> hashSet = new HashSet<int>();
    private readonly IntHashSet intHashSet = new IntHashSet();
    
    //[Benchmark]
    public void HashSet_Add() {
        for (var i = 0; i < 128; i++) {
            this.hashSet.Add(i);
        }
        
        for (var i = 0; i < 128; i++) {
            this.hashSet.Remove(i);
        }
    }
    
    //[Benchmark]
    public void IntHashSet_Add() {
        for (var i = 0; i < 128; i++) {
            this.intHashSet.Add(i);
        }
        
        for (var i = 0; i < 128; i++) {
            this.intHashSet.Remove(i);
        }
    }
}