namespace Scellecs.Morpeh.Collections {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static unsafe class IntStackExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push(this IntStack stack, in int value) {
            if (stack.length == stack.capacity) {
                ArrayHelpers.Grow(ref stack.data, stack.capacity <<= 1);
            }

            fixed (int* d = &stack.data[0]) {
                *(d + stack.length++) = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Pop(this IntStack stack) {
            fixed (int* d = &stack.data[0]) {
                return *(d + --stack.length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(this IntStack stack) {
            stack.data   = null;
            stack.length = stack.capacity = 0;
        }
    }
}