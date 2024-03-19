namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    using UnityEngine;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static unsafe class IntFastListExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Add(this IntFastList list) {
            var index = list.length;
            if (++list.length == list.capacity) {
                list.data.Resize(list.capacity = HashHelpers.GetCapacity(list.capacity) + 1);
            }

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Get(this IntFastList list, in int index) => list.data.ptr[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(this IntFastList list, in int index, in int value) {
            list.data.ptr[index] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Add(this IntFastList list, in int value) {
            var index = list.length;
            if (++list.length == list.capacity) {
                list.data.Resize(list.capacity = HashHelpers.GetCapacity(list.capacity) + 1);
            }

            list.data.ptr[index] = value;

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddListRange(this IntFastList list, IntFastList other) {
            if (other.length > 0) {
                var newSize = list.length + other.length;
                if (newSize > list.capacity) {
                    list.data.Resize(list.capacity = HashHelpers.GetCapacity(newSize - 1) + 1);
                }

                Array.Copy(other.data.data, 0, list.data.data, list.length, other.length);

                list.length += other.length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf(this IntFastList list, int value) => ArrayHelpers.IndexOfUnsafeInt(list.data.ptr, list.data.data.Length, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(this IntFastList list, int value) => list.RemoveAt(list.IndexOf(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSwap(this IntFastList list, int value, out IntFastList.ResultSwap swap) => list.RemoveAtSwap(list.IndexOf(value), out swap);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveAt(this IntFastList list, int index) {
            --list.length;
            if (index < list.length) {
                Array.Copy(list.data.data, index + 1, list.data.data, index, list.length - index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveAtSwap(this IntFastList list, int index, out IntFastList.ResultSwap swap) {
            if (list.length-- > 1) {
                swap.oldIndex = list.length;
                swap.newIndex = index;
                list.data.ptr[swap.newIndex] = list.data.ptr[swap.oldIndex];

                return true;
            }

            swap = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(this IntFastList list) {
            if (list.length <= 0) {
                return;
            }

            Array.Clear(list.data.data, 0, list.length);
            list.length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort(this IntFastList list) => Array.Sort(list.data.data, 0, list.length, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort(this IntFastList list, int index, int len) => Array.Sort(list.data.data, index, len, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int[] ToArray(this IntFastList list) {
            var newArray = new int[list.length];
            Array.Copy(list.data.data, 0, newArray, 0, list.length);
            return newArray;
        }
    }
}