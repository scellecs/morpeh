#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Collections;

    public static class NativeArchetypeExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe NativeArchetype AsNative(this Archetype archetype) {
            if (archetype.usedInNative == false) {
                var list = archetype.entitiesNative = new FastList<int>(archetype.entities.count);
                
                foreach (var entityId in archetype.entities) {
                    archetype.world.entities[entityId].indexInCurrentArchetype = list.Add(entityId);
                }

                archetype.entities     = null;
                archetype.usedInNative = true;
            }
            
            var nativeArchetype = new NativeArchetype {
                entities = archetype.entitiesNative.AsNative()
            };

            fixed (int* lengthPtr = &archetype.length) {
                nativeArchetype.lengthPtr = lengthPtr;
            }

            return nativeArchetype;
        }
    }
}
#endif