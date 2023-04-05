namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    [Il2CppEagerStaticClassConstruction]
    internal static class ArrayHelpers {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Grow<T>(ref T[] array, int newSize) {
            var newArray = new T[newSize];
            Array.Copy(array, 0, newArray, 0, array.Length);
            array = newArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(T[] array, T value, EqualityComparer<T> comparer) {
            for (int i = 0, length = array.Length; i < length; ++i) {
                if (comparer.Equals(array[i], value)) {
                    return i;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe int IndexOfUnsafeInt(int* array, int length, int value) {
            var i = 0;
            for (int* current = array, len = array + length; current < len; ++current) {
                if (*current == value) {
                    return i;
                }

                ++i;
            }

            return -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void InsertionSort(this int[] a, int l, int size)
        {
            var r = l + size;
            for (var i = l + 1; i < r; i++)
            {
                ref var ie = ref a[i];
                ref var im1e = ref a[i - 1];
                if (ie < im1e)
                {
                    var currentElement = ie;
                    ie = im1e;
                    var j = i - 1;
                    for (; j > l && (currentElement < a[j - 1]); j--)
                    {
                        a[j] = a[j - 1];
                    }
                    a[j] = currentElement;
                }
            }
        }
    }
}