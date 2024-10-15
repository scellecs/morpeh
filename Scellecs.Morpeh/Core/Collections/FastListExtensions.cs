namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public static class FastListExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Expand<T>(this FastList<T> list) where T : unmanaged {
            ArrayHelpers.Grow(ref list.data, list.capacity = HashHelpers.GetCapacity(list.capacity) + 1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Resize<T>(this FastList<T> list, int newCapacity) where T : unmanaged {
            ArrayHelpers.Grow(ref list.data, list.capacity = HashHelpers.GetCapacity(newCapacity - 1) + 1);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Add<T>(this FastList<T> list) {
            var index = list.length;
            if (++list.length == list.capacity) {
                ArrayHelpers.Grow(ref list.data, list.capacity = HashHelpers.GetCapacity(list.capacity) + 1);
            }

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Add<T>(this FastList<T> list, T value) {
            var index = list.length;
            if (++list.length == list.capacity) {
                ArrayHelpers.Grow(ref list.data, list.capacity = HashHelpers.GetCapacity(list.capacity) + 1);
            }

            list.data[index] = value;
            return index;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FastList<T> WithElement<T>(this FastList<T> list, T value) {
            list.Add(value);
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddListRange<T>(this FastList<T> list, FastList<T> other) {
            if (other.length > 0) {
                var newSize = list.length + other.length;
                
                if (newSize > list.capacity) {
                    ArrayHelpers.Grow(ref list.data, list.capacity = HashHelpers.GetCapacity(newSize - 1) + 1);
                }
                
                Array.Copy(other.data, 0, list.data, list.length, other.length);

                list.length += other.length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(this FastList<T> list, int source, int destination)
        {
            list.data[destination] = list.data[source];
        } 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<T>(this FastList<T> list, T value) {
            for (int i = 0, length = list.length; i < length; i++) {
                if (list.comparer.Equals(value, list.data[i])) {
                    return i;
                }
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove<T>(this FastList<T> list, T value) => list.RemoveAt(list.IndexOf(value));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSave<T>(this FastList<T> list, T value) {
            var index = list.IndexOf(value);
            if (index < 0) {
                return;
            }
            list.RemoveAt(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSwap<T>(this FastList<T> list, T value, out ResultSwap swap) {
            list.RemoveAtSwap(list.IndexOf(value), out swap);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveSwapSave<T>(this FastList<T> list, T value, out ResultSwap swap) {
            var index = list.IndexOf(value);
            if (index < 0) {
                swap = default;
                return;
            }
            list.RemoveAtSwap(index, out swap);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveAt<T>(this FastList<T> list, int index) {
            --list.length;
            if (index < list.length) {
                Array.Copy(list.data, index + 1, list.data, index, list.length - index);
            }

            list.data[list.length] = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveAtSwap<T>(this FastList<T> list, int index, out ResultSwap swap) {
            var lastIndex = list.length - 1;

            if (index < 0 || index > lastIndex) {
                swap = default;
                return false;
            }

            swap = new ResultSwap {
                oldIndex = lastIndex,
                newIndex = index
            };

            if (index < lastIndex) {
                list.data[index] = list.data[lastIndex];
            }
            
            list.data[lastIndex] = default;
            list.length--;

            return true;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RemoveAtSwap<T>(this FastList<T> list, int index, out T newValue) {
            var lastIndex = list.length - 1;
    
            if (index < 0 || index > lastIndex) {
                newValue = default;
                return false;
            }
    
            newValue = list.data[index] = index < lastIndex ? list.data[lastIndex] : default;
            list.data[lastIndex] = default;
            list.length--;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear<T>(this FastList<T> list) {
            if (list.length <= 0) {
                return;
            }

            Array.Clear(list.data, 0, list.length);
            list.length = 0;
        }

        //todo rework
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T>(this FastList<T> list) => Array.Sort(list.data, 0, list.length, null);

        //todo rework
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Sort<T>(this FastList<T> list, int index, int len) => Array.Sort(list.data, index, len, null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] ToArray<T>(this FastList<T> list) {
            var newArray = new T[list.length];
            Array.Copy(list.data, 0, newArray, 0, list.length);
            return newArray;
        }
    }
}