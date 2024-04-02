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
            archetype.hash = default;
            
            archetype.length  = 0;
            
            archetype.components?.Clear();
            archetype.components = null;

            archetype.entities.Dispose();
            archetype.entities = default;
            
            archetype.filters?.Clear();
            archetype.filters = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Add(this Archetype archetype, Entity entity) {
            if (archetype.length == archetype.capacity) {
                archetype.capacity <<= 1;
                archetype.entities.Resize(archetype.capacity);
            }
            
            archetype.entities[archetype.length] = entity;
            return archetype.length++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(this Archetype archetype, int index) {
            archetype.entities[index] = archetype.entities[archetype.length - 1];
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
