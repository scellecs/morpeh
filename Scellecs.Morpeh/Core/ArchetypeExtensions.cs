namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Collections;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class ArchetypeExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Dispose(this Archetype archetype) {
            archetype.id = ArchetypeId.Invalid;
            
            archetype.length  = 0;
            
            archetype.components?.Clear();
            archetype.components = null;
            
            archetype.entities?.Clear();
            archetype.entities = null;
            
            archetype.filters?.Clear();
            archetype.filters = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this Archetype archetype, Entity entity) {
            if (archetype.entities.Set(entity.entityId.id)) {
                MLogger.LogTrace($"[ArchetypeExtensions] Added entity {entity} to archetype {archetype.id}");
                archetype.length++;
            } else {
                MLogger.LogTrace($"[ArchetypeExtensions] Entity {entity} already in archetype {archetype.id}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(this Archetype archetype, Entity entity) {
            if (archetype.entities.Unset(entity.entityId.id)) {
                MLogger.LogTrace($"[ArchetypeExtensions] Removed entity {entity} from archetype {archetype.id}");
                archetype.length--;
            } else {
                MLogger.LogTrace($"[ArchetypeExtensions] Entity {entity} not found in archetype {archetype.id}");
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this Archetype archetype) {
            return archetype.length == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFilter(this Archetype archetype, Filter filter) {
            if (archetype.filters.Add(filter.id, filter, out _)) {
                MLogger.LogTrace($"[ArchetypeExtensions] Added filter {filter} to archetype {archetype.id}");
            } else {
                MLogger.LogTrace($"[ArchetypeExtensions] Filter {filter} already in archetype {archetype.id}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveFilter(this Archetype archetype, Filter filter) {
            if (archetype.filters.Remove(filter.id, out _)) {
                MLogger.LogTrace($"[ArchetypeExtensions] Removed filter {filter} from archetype {archetype.id}");
            } else {
                MLogger.LogTrace($"[ArchetypeExtensions] Filter {filter} not found in archetype {archetype.id}");
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearFilters(this Archetype archetype) {
            archetype.filters.Clear();
            MLogger.LogTrace($"[ArchetypeExtensions] Removed all filters from archetype {archetype.id}");
        }
    }
}
