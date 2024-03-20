using BenchmarkDotNet.Attributes;
using Scellecs.Morpeh;

namespace Benchmarks;

[MemoryDiagnoser(false)]
public class TransientUtilityBenchmarks {
    private TransientArchetype transient;
    private TypeInfo[] typeInfos;
    
    [Params(64)]
    public int Count;
    
    [GlobalSetup]
    public void SetUp() {
        TransientUtility.Initialize(ref this.transient);
        this.typeInfos = new TypeInfo[this.Count];
        for (var i = 0; i < this.Count; i++) {
            this.typeInfos[i] = new TypeInfo(new TypeOffset(i), new TypeId(i));
        }
    }
    
    [Benchmark]
    public void Rebase() {
        for (var i = 0; i < this.Count; i++) {
            TransientUtility.Rebase(ref this.transient, null);
        }
    }
    
    [Benchmark]
    public void MultipleAdd() {
        for (var i = 0; i < this.Count; i++) {
            var typeInfo = this.typeInfos[i];
            TransientUtility.AddComponent(ref this.transient, ref typeInfo);
        }
        
        TransientUtility.Rebase(ref this.transient, null);
    }
    
    [Benchmark]
    public void MultipleRemove() {
        for (var i = 0; i < this.Count; i++) {
            var typeInfo = this.typeInfos[i];
            TransientUtility.RemoveComponent(ref this.transient, ref typeInfo);
        }
        
        TransientUtility.Rebase(ref this.transient, null);
    }
    
    [Benchmark]
    public void AddRemove() {
        for (var i = 0; i < this.Count; i++) {
            var typeInfo = this.typeInfos[i];
            TransientUtility.AddComponent(ref this.transient, ref typeInfo);
            TransientUtility.RemoveComponent(ref this.transient, ref typeInfo);
        }
        
        TransientUtility.Rebase(ref this.transient, null);
    }
}