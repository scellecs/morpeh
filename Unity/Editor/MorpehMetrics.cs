#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER && MORPEH_METRICS
using Unity.Profiling; 
using Unity.Profiling.Editor; 

[System.Serializable] 
[ProfilerModuleMetadata("Morpeh")] 
public class CreatureBatchesProfilerModule : ProfilerModule 
{ 
    private static readonly ProfilerCounterDescriptor[] 
        counters = { 
            new("Entities", ProfilerCategory.Scripts), 
            new("Archetypes", ProfilerCategory.Scripts), 
            new("Filters", ProfilerCategory.Scripts), 
            new("Systems", ProfilerCategory.Scripts), 
            new("Commits", ProfilerCategory.Scripts), 
            new("Migrations", ProfilerCategory.Scripts), 
        }; 

    public CreatureBatchesProfilerModule() : base(counters) { }
}
#endif