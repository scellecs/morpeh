using System.Text;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

public static class TestExtensions {
    internal static ArchetypeId With<T>(this ArchetypeId archetype) where T : struct, IComponent {
        var id = TypeIdentifier<T>.info.id;
        return archetype.Combine(id);
    }
    
    internal static Archetype GetArchetype(this World world, ArchetypeId archetypeId) {
        world.archetypes.TryGetValue(archetypeId.GetValue(), out var archetype);
        return archetype;
    }
    
    internal static int ArchetypeLengthOf(this World world, ArchetypeId archetypeId) {
        world.archetypes.TryGetValue(archetypeId.GetValue(), out var archetype);
        return archetype?.length ?? 0;
    }
    
    internal static string DumpFilterArchetypes(this Filter filter) {
        var sb = new StringBuilder();
        sb.Append($"Filter {filter} archetypes:");
        foreach (var archetype in filter.archetypes) {
            sb.Append($" {archetype.id}");
        }
        return sb.ToString();
    }
    
    internal static void DumpFilterArchetypes(this Filter filter, ITestOutputHelper output) {
        output.WriteLine(DumpFilterArchetypes(filter));
    }
}