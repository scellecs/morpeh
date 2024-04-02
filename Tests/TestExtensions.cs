using System.Text;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;
using Xunit.Abstractions;

namespace Tests;

public static class TestExtensions {
    internal static ArchetypeHash With<T>(this ArchetypeHash archetype) where T : struct, IComponent {
        var id = TypeIdentifier<T>.info.id;
        return archetype.Combine(id);
    }
    
    internal static Archetype GetArchetype(this World world, ArchetypeHash archetypeHash) {
        world.archetypes.TryGetValue(archetypeHash.GetValue(), out var archetype);
        return archetype;
    }
    
    internal static int ArchetypeLengthOf(this World world, ArchetypeHash archetypeHash) {
        world.archetypes.TryGetValue(archetypeHash.GetValue(), out var archetype);
        return archetype?.length ?? 0;
    }
    
    internal static string DumpFilterArchetypes(this Filter filter) {
        var sb = new StringBuilder();
        sb.Append($"Filter {filter} archetypes:");
        for (var i = 0; i < filter.archetypesLength; i++) {
            var archetype = filter.archetypes[i];
            sb.Append($"\n  {archetype}");
        }
        return sb.ToString();
    }
    
    internal static void DumpFilterArchetypes(this Filter filter, ITestOutputHelper output) {
        output.WriteLine(DumpFilterArchetypes(filter));
    }
    
    internal static ArchetypeHash ArchetypeOf(this World world, Entity entity) {
        if (world.IsDisposed(entity)) {
            return default(ArchetypeHash);
        }

        var archetype = world.entities[entity.Id].currentArchetype;
        return archetype?.hash ?? default(ArchetypeHash);
    }
}