namespace Scellecs.Morpeh {
    using System.Runtime.CompilerServices;
    using Collections;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    internal static class ArchetypeExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Ctor(this Archetype archetype) {
            archetype.world   = World.worlds.data[archetype.worldId];
            archetype.filters = new FastList<Filter>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Dispose(this Archetype archetype) {
            archetype.usedInNative = false;
            
            archetype.id      = -1;
            archetype.length  = -1;

            archetype.typeIds = null;
            archetype.world   = null;

            archetype.entities?.Clear();
            archetype.entities = null;
            
            archetype.entitiesNative?.Clear();
            archetype.entitiesNative = null;

            archetype.addTransfer.Clear();
            archetype.addTransfer = null;

            archetype.removeTransfer.Clear();
            archetype.removeTransfer = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this Archetype archetype, Entity entity) {
            archetype.length++;
            
            if (archetype.usedInNative) {
                entity.indexInCurrentArchetype = archetype.entitiesNative.Add(entity.entityId.id);
            }
            else {
                archetype.entities.Set(entity.entityId.id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(this Archetype archetype, Entity entity) {
            archetype.length--;

            if (archetype.usedInNative) {
                var index = entity.indexInCurrentArchetype;
                if (archetype.entitiesNative.RemoveAtSwap(index, out int newValue)) {
                    archetype.world.entities[newValue].indexInCurrentArchetype = index;
                }
            }
            else {
                archetype.entities.Unset(entity.entityId.id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFilter(this Archetype archetype, Filter filter) {
            archetype.filters.Add(filter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveFilter(this Archetype archetype, Filter filter) {
            archetype.filters.Remove(filter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddTransfer(this Archetype archetype, int typeId, out int archetypeId, out Archetype newArchetype) {
            if (archetype.addTransfer.TryGetValue(typeId, out archetypeId)) {
                newArchetype = archetype.world.archetypes.data[archetypeId];
            }
            else {
                newArchetype = archetype.world.GetArchetype(archetype.typeIds, typeId, true, out archetypeId);
                archetype.addTransfer.Add(typeId, archetypeId, out _);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveTransfer(this Archetype archetype, int typeId, out int archetypeId, out Archetype newArchetype) {
            if (archetype.removeTransfer.TryGetValue(typeId, out archetypeId)) {
                newArchetype = archetype.world.archetypes.data[archetypeId];
            }
            else {
                newArchetype = archetype.world.GetArchetype(archetype.typeIds, typeId, false, out archetypeId);
                archetype.removeTransfer.Add(typeId, archetypeId, out _);
            }
        }
    }
}
