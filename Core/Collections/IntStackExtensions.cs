namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static unsafe class IntStackExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push(this IntStack stack, in int value) {
            if (stack.length == stack.capacity) {
                stack.data.Resize(stack.capacity = HashHelpers.GetCapacity(stack.capacity) + 1);
            }

            stack.data.ptr[stack.length++] = value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PushRange(this IntStack stack, IntStack other) {
            if (other.length > 0) {
                var newSize = stack.length + other.length;
                if (newSize > stack.capacity) {
                    stack.data.Resize(stack.capacity = HashHelpers.GetCapacity(newSize) + 1);
                }
                
                Array.Copy(other.data.data, 0, stack.data.data, stack.length, other.length);

                stack.length += other.length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Pop(this IntStack stack) => stack.data.ptr[--stack.length];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(this IntStack stack) {
            stack.data.Clear();
            stack.length = stack.capacity = 0;
        }
    }
}