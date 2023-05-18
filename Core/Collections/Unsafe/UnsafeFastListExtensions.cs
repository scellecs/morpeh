namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static unsafe class UnsafeFastListExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Expand<T>(this UnsafeFastList<T> list) where T : unmanaged {
            list.data.Resize(list.capacity = HashHelpers.GetCapacity(list.capacity) + 1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Resize<T>(this UnsafeFastList<T> list, int newCapacity) where T : unmanaged {
            list.data.Resize(list.capacity = HashHelpers.GetCapacity(newCapacity) + 1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Add<T>(this UnsafeFastList<T> list) where T : unmanaged {
            var index = list.length;
            if (++list.length == list.capacity) {
                list.data.Resize(list.capacity = HashHelpers.GetCapacity(list.capacity) + 1);
            }

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Add<T>(this UnsafeFastList<T> list, T value) where T : unmanaged  {
            var index = list.length;
            if (++list.length == list.capacity) {
                list.data.Resize(list.capacity = HashHelpers.GetCapacity(list.capacity) + 1);
            }

            list.data.ptr[index] = value;
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddListRange<T>(this UnsafeFastList<T> list, UnsafeFastList<T> other) where T : unmanaged  {
            if (other.length > 0) {
                var newSize = list.length + other.length;
                if (newSize > list.capacity) {
                    list.data.Resize(list.capacity = HashHelpers.GetCapacity(newSize) + 1);
                }

                if (list == other) {
                    Array.Copy(list.data.data, 0, list.data.data, list.length, list.length);
                }
                else {
                    Array.Copy(other.data.data, 0, list.data.data, list.length, other.length);
                }

                list.length += other.length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(this UnsafeFastList<T> list, int source, int destination) where T : unmanaged 
        {
            list.data.ptr[destination] = list.data.ptr[source];
            list.lastSwappedIndex = destination;
        } 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this UnsafeFastList<T> list, T value) where T : unmanaged  => ArrayHelpers.IndexOf(list.data.data, value, list.comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove<T>(this UnsafeFastList<T> list, T value) where T : unmanaged  => list.RemoveAt(list.IndexOf(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSwap<T>(this UnsafeFastList<T> list, T value, out UnsafeFastList<T>.ResultSwap swap) where T : unmanaged  => list.RemoveAtSwap(list.IndexOf(value), out swap);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveAt<T>(this UnsafeFastList<T> list, int index) where T : unmanaged  {
            --list.length;
            if (index < list.length) {
                Array.Copy(list.data.data, index + 1, list.data.data, index, list.length - index);
            }

            list.data.ptr[list.length] = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveAtSwap<T>(this UnsafeFastList<T> list, int index, out UnsafeFastList<T>.ResultSwap swap) where T : unmanaged  {
            if (list.length-- > 1) {
                swap.oldIndex = list.length;
                swap.newIndex = index;

                list.data.ptr[swap.newIndex] = list.data.ptr[swap.oldIndex];
                list.data.ptr[swap.oldIndex] = default;
                list.lastSwappedIndex    = index;
                return true;
            }

            list.lastSwappedIndex = -1;
            swap = default;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveAtSwap<T>(this UnsafeFastList<T> list, int index, out T newValue) where T : unmanaged  {
            if (list.length-- > 1) {
                var oldIndex = list.length;
                newValue = list.data.ptr[index] = list.data.ptr[oldIndex];
                list.lastSwappedIndex = index;
                return true;
            }

            list.lastSwappedIndex = -1;
            newValue = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this UnsafeFastList<T> list) where T : unmanaged  {
            if (list.length <= 0) {
                return;
            }

            Array.Clear(list.data.data, 0, list.length);
            list.length = 0;
            list.lastSwappedIndex = -1;
        }

        //todo rework
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T>(this UnsafeFastList<T> list) where T : unmanaged  => Array.Sort(list.data.data, 0, list.length, null);

        //todo rework
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T>(this UnsafeFastList<T> list, int index, int len) where T : unmanaged  => Array.Sort(list.data.data, index, len, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ToArray<T>(this UnsafeFastList<T> list) where T : unmanaged  {
            var newArray = new T[list.length];
            Array.Copy(list.data.data, 0, newArray, 0, list.length);
            return newArray;
        }
    }
}
