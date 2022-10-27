#if MORPEH_BURST
namespace Morpeh.Native {
    using System.Runtime.CompilerServices;
    using Collections;

    public static class NativeIntFastListExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe NativeIntFastList AsNative(this IntFastList fastList) {
            var nativeCollection = new NativeIntFastList();
            
            fixed (int* lengthPtr = &fastList.length)
            fixed (int* capacityPtr = &fastList.capacity)
            fixed (int* dataPtr = &fastList.data[0]) {
                nativeCollection.lengthPtr   = lengthPtr;
                nativeCollection.capacityPtr = capacityPtr;
                nativeCollection.data        = dataPtr;
            }

            return nativeCollection;
        }
    }
}
#endif