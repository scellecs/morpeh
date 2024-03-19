using Scellecs.Morpeh;
using Scellecs.Morpeh.Collections;

namespace Tests;

public static class TestExtensions
{
    internal static ArchetypeId With<T>(this ArchetypeId archetype) where T : struct, IComponent
    {
        return archetype.Combine(TypeIdentifier<T>.info.id);
    }
    
    internal static Archetype GetArchetype(this World world, ArchetypeId archetypeId) {
        world.archetypes.TryGetValue(archetypeId.GetValue(), out var archetype);
        return archetype;
    }
}