namespace Morpeh {
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
            archetype.id      = -1;
            archetype.length  = -1;
            archetype.isDirty = false;

            archetype.typeIds = null;
            archetype.world   = null;

            archetype.entitiesBitMap.Clear();
            archetype.entitiesBitMap = null;

            archetype.addTransfer.Clear();
            archetype.addTransfer = null;

            archetype.removeTransfer.Clear();
            archetype.removeTransfer = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this Archetype archetype, Entity entity) {
            archetype.length++;
            entity.indexInCurrentArchetype = archetype.entitiesBitMap.Add(entity.internalID);

            archetype.isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(this Archetype archetype, Entity entity) {
            archetype.length--;
            var index = entity.indexInCurrentArchetype;
            if (archetype.entitiesBitMap.RemoveAtSwap(index, out int newValue)) {
                archetype.world.entities[newValue].indexInCurrentArchetype = index;
            }
            archetype.isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddFilter(this Archetype archetype, Filter filter) {
            archetype.filters.Add(filter);
            archetype.isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveFilter(this Archetype archetype, Filter filter) {
            archetype.filters.Remove(filter);
            archetype.isDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Process(this Archetype archetype) {
            for (int i = 0, len = archetype.filters.length; i < len; i++) {
                archetype.filters.data[i].isDirty = true;
            }

            archetype.isDirty = false;
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
