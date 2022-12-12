#if MORPEH_BURST
namespace Scellecs.Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Collections;

    public static unsafe class NativeFastListExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeFastList<TNative> AsNative<TNative>(this FastList<TNative> fastList) where TNative : unmanaged {
            var nativeIntHashMap = new NativeFastList<TNative>();
            
            fixed (int* lengthPtr = &fastList.length)
            fixed (int* capacityPtr = &fastList.capacity)
            fixed (TNative* dataPtr = &fastList.data[0]) {
                nativeIntHashMap.lengthPtr   = lengthPtr;
                nativeIntHashMap.capacityPtr = capacityPtr;
                nativeIntHashMap.data        = dataPtr;
            }

            return nativeIntHashMap;
        }

        public static ref TNative GetRef<TNative>(this NativeFastList<TNative> fastList, in int index) where TNative : unmanaged => ref *(fastList.data + index);
    }
}
#endif