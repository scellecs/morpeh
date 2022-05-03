#if MORPEH_BURST
namespace Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Morpeh;

    public static class NativeArchetypeExtensions {
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