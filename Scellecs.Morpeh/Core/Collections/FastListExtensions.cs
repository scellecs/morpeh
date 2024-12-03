namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class FastListExtensions {
        /// <summary>
        /// Ensures the capacity of the list is at least the specified value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Grow<T>(this FastList<T> list, int newCapacity) {
            var capacity = HashHelpers.GetCapacitySmall(newCapacity - 1) + 1;
            if (capacity > list.capacity) {
                list.capacity = capacity;
                ArrayHelpers.Grow(ref list.data, list.capacity);
            }
        }

        /// <summary>
        /// Adds a new element to the end of the list.
        /// </summary>
        /// <returns>The index at which the element was added.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Add<T>(this FastList<T> list, T value) {
            var index = list.length;
            if (++list.length == list.capacity) {
                ArrayHelpers.Grow(ref list.data, list.capacity = HashHelpers.GetCapacitySmall(list.capacity) + 1);
            }

            list.data[index] = value;
            return index;
        }

        /// <summary>
        /// Adds all elements from another list to the end of the current list.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this FastList<T> list, FastList<T> other) {
            if (other.length > 0) {
                var newSize = list.length + other.length;

                if (newSize > list.capacity) {
                    ArrayHelpers.Grow(ref list.data, list.capacity = HashHelpers.GetCapacitySmall(newSize - 1) + 1);
                }

                Array.Copy(other.data, 0, list.data, list.length, other.length);
                list.length += other.length;
            }
        }

        /// <summary>
        /// Searches for the specified element in the list and returns the index of its first occurrence.
        /// </summary>
        /// <returns>The zero-based index of the first occurrence of the element; or -1 if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this FastList<T> list, T value) {
            for (int i = 0, length = list.length; i < length; i++) {
                if (list.comparer.Equals(value, list.data[i])) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Removes the first occurrence of a specific element from the list, if the element is found.
        /// </summary>
        /// <returns>true if the element was found and removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Remove<T>(this FastList<T> list, T value) {
            var index = list.IndexOf(value);
            var shouldRemove = index >= 0;
            if (shouldRemove) {
                list.RemoveAtFast(index);
            }

            return shouldRemove;
        }

        /// <summary>
        /// Removes the element at the specified index of the list.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveAt<T>(this FastList<T> list, int index) {
            if (index < 0 || index >= list.length) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            --list.length;
            Array.Copy(list.data, index + 1, list.data, index, list.length - index);
            list.data[list.length] = default;
        }

        /// <summary>
        /// Removes the element at the specified index of the list.
        /// Does not perform bounds checking.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveAtFast<T>(this FastList<T> list, int index) {
            --list.length;
            Array.Copy(list.data, index + 1, list.data, index, list.length - index);
            list.data[list.length] = default;
        }

        /// <summary>
        /// Removes the first occurrence of a specific element from the list by overwriting it with the last element, 
        /// and decrements the size of the list if the element is found.
        /// </summary>
        /// <returns>true if the element was found and removed; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveSwapBack<T>(this FastList<T> list, T value) {
            var index = list.IndexOf(value);
            var shouldRemove = index >= 0;
            if (shouldRemove) {
                list.RemoveAtSwapBackFast(index);
            }

            return shouldRemove;
        }

        /// <summary>
        /// Removes the element at the specified index by overwriting it with the last element in the list, and decrements the size of the list.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the index is out of range.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveAtSwapBack<T>(this FastList<T> list, int index) {
            if (index < 0 || index >= list.length) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var lastIndex = list.length - 1;
            list.data[index] = list.data[lastIndex];
            list.data[lastIndex] = default;
            list.length--;
        }

        /// <summary>
        /// Removes the element at the specified index by overwriting it with the last element in the list, and decrements the size of the list.
        /// Does not perform bounds checking.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveAtSwapBackFast<T>(this FastList<T> list, int index) {
            var lastIndex = list.length - 1;
            list.data[index] = list.data[lastIndex];
            list.data[lastIndex] = default;
            list.length--;
        }

        /// <summary>
        /// Removes a range of elements from the list.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the index is out of range or if the count is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the range exceeds the bounds of the list.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveRange<T>(this FastList<T> list, int index, int count) {
            if (index < 0 || index >= list.length) {
                 throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0) {
                throw new ArgumentOutOfRangeException(nameof(count), "Non-negative number required");
            }

            var elementsToMove = list.length - (index + count);
            if (elementsToMove < 0) {
                throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.", nameof(count));
            }

            if (elementsToMove > 0) {
                Array.Copy(list.data, index + count, list.data, index, elementsToMove);
            }

            list.length -= count;
        }

        /// <summary>
        /// Swaps the elements at the specified source and destination indices.
        /// Does not perform bounds checking.
        /// </summary>
        /// <param name="source">The zero-based index of the first element.</param>
        /// <param name="destination">The zero-based index of the second element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SwapFast<T>(this FastList<T> list, int source, int destination) {
            var temp = list.data[source];
            list.data[source] = list.data[destination];
            list.data[destination] = temp;
        }

        /// <summary>
        /// Clears all elements from the list.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this FastList<T> list) {
            if (list.length <= 0) {
                return;
            }

            Array.Clear(list.data, 0, list.length);
            list.length = 0;
        }

        /// <summary>
        /// Copies the elements of the list to the specified array.
        /// </summary>
        /// <param name="array">The destination array.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this FastList<T> list, T[] array) {
            Array.Copy(list.data, 0, array, 0, list.length);
        }

        /// <summary>
        /// Copies the elements of the list to a new array.
        /// </summary>
        /// /// <returns>
        /// A new array containing the elements of the list. 
        /// If the list is empty, returns <see cref="Array.Empty{T}"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ToArray<T>(this FastList<T> list) {
            var shouldCopy = list.length > 0;
            var newArray = shouldCopy ? new T[list.length] : Array.Empty<T>();
            if (shouldCopy) {
                list.CopyTo(newArray);
            }
            return newArray;
        }

        /// <summary>
        /// Sorts the elements of the list using the specified comparer.
        /// If no comparer is provided, it uses <see cref="Comparer{T}.Default"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T>(this FastList<T> list, IComparer<T> comparer = null) {
            Array.Sort(list.data, 0, list.length, comparer ?? Comparer<T>.Default);
        }

        /// <summary>
        /// Sorts a range of elements in the list using the specified comparer.
        /// If no comparer is provided, it uses <see cref="Comparer{T}.Default"/>.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range to sort.</param>
        /// <param name="count">The number of elements to sort.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the index is out of range or if the count is negative.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the range exceeds the bounds of the list.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T>(this FastList<T> list, int index, int count, IComparer<T> comparer = null) {
            if (index < 0 || index >= list.length) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0) {
                throw new ArgumentOutOfRangeException(nameof(count), "Non-negative number required.");
            }

            if (index + count > list.length) {
                throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.");
            }

            Array.Sort(list.data, index, count, comparer ?? Comparer<T>.Default);
        }
    }
}