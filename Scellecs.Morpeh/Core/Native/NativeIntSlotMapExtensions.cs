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
            var pageIndex = (uint)key >> IntSlotMap.PAGE_SHIFT;
            if (pageIndex < (uint)slotMap.pagesLength) {
                var page = slotMap.pages[pageIndex];
                if (page != null) {
                    return page[key & IntSlotMap.PAGE_MASK] - 1;
                }
            }

            return -1;
        }
    }
}
#endif