#if UNITY_2019_1_OR_NEWER
namespace Morpeh.Core.NativeCollections {
    using System.Runtime.CompilerServices;
    using Morpeh;

    public static class ArchetypeExtensionsForNative {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe NativeArchetypeWrapper AsNative(this Archetype archetype) {
            var nativeArchetype = new NativeArchetypeWrapper {
                entitiesBitMap = archetype.entitiesBitMap.AsNative()
            };

            fixed (int* lengthPtr = &archetype.length) {
                nativeArchetype.lengthPtr = lengthPtr;
            }

            return nativeArchetype;
        }
    }
}
#endif