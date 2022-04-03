#if MORPEH_BURST
namespace Morpeh.NativeCollections {
    using System.Runtime.CompilerServices;
    using Collections;

    public static class FastListExtensionsForNative {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NativeFastListWrapper<TNative> AsNative<TNative>(this FastList<TNative> fastList) where TNative : unmanaged {
            var nativeIntHashMap = new NativeFastListWrapper<TNative>();
            
            fixed (int* lengthPtr = &fastList.length)
            fixed (int* capacityPtr = &fastList.capacity)
            fixed (TNative* dataPtr = &fastList.data[0]) {
                nativeIntHashMap.lengthPtr   = lengthPtr;
                nativeIntHashMap.capacityPtr = capacityPtr;
                nativeIntHashMap.data        = dataPtr;
            }

            return nativeIntHashMap;
        }
    }
}
#endif