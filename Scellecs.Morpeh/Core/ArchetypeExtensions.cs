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
        public static void Add(this Archetype archetype, EntityId entityId) {
            archetype.entities.Set(entityId.id);
            ++archetype.length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(this Archetype archetype, EntityId entityId) {
            archetype.entities.Unset(entityId.id);
            --archetype.length;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEmpty(this Archetype archetype) {
            return archetype.length == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFilter(this Archetype archetype, Filter filter) {
            archetype.filters.Add(filter.id, filter, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveFilter(this Archetype archetype, Filter filter) {
            archetype.filters.Remove(filter.id, out _);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearFilters(this Archetype archetype) {
            archetype.filters.Clear();
        }
    }
}
