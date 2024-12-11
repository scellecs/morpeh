#if MORPEH_BURST
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Scellecs.Morpeh.Native {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static unsafe class NativeIntSlotMapExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(this ref NativeIntSlotMap slotMap, in int key) {
            var rem = key & *slotMap.capacityMinusOnePtr;
            int next;
            for (var i = slotMap.buckets[rem] - 1; i >= 0; i = next) {
                ref var slot = ref slotMap.slots[i];
                if (slot.key - 1 == key) {
                    return i;
                }

                next = slot.next;
            }

            return -1;
        }
    }
}
#endif