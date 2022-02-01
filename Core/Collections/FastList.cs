namespace Morpeh.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
#if UNITY_2019_1_OR_NEWER
    using Core.NativeCollections;
#endif
    
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FastList<T> : IEnumerable<T> {
        public T[] data;
        public int length;
        public int capacity;

        public EqualityComparer<T> comparer;
        
#if UNITY_2019_1_OR_NEWER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe NativeFastList<TNative> AsNative<TNative>() where TNative : unmanaged {
            var nativeIntHashMap = new NativeFastList<TNative>();
            
            fixed (int* lengthPtr = &this.length)
            fixed (int* capacityPtr = &this.capacity)
            fixed (TNative* dataPtr = &(this.data as TNative[])[0]) {
                nativeIntHashMap.lengthPtr   = lengthPtr;
                nativeIntHashMap.capacityPtr = capacityPtr;
                nativeIntHashMap.data        = dataPtr;
            }

            return nativeIntHashMap;
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastList() {
            this.capacity = 3;
            this.data     = new T[this.capacity];
            this.length   = 0;

            this.comparer = EqualityComparer<T>.Default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastList(int capacity) {
            this.capacity = HashHelpers.GetCapacity(capacity);
            this.data     = new T[this.capacity];
            this.length   = 0;

            this.comparer = EqualityComparer<T>.Default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastList(FastList<T> other) {
            this.capacity = other.capacity;
            this.data     = new T[this.capacity];
            this.length   = other.length;
            Array.Copy(other.data, 0, this.data, 0, this.length);

            this.comparer = other.comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.list    = this;
            e.current = default;
            e.index   = 0;
            return e;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct ResultSwap {
            public int oldIndex;
            public int newIndex;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator : IEnumerator<T> {
            public FastList<T> list;

            public T   current;
            public int index;

            public bool MoveNext() {
                if (this.index >= this.list.length) {
                    return false;
                }

                this.current = this.list.data[this.index++];
                return true;
            }

            public void Reset() {
                this.index   = 0;
                this.current = default;
            }

            public T           Current => this.current;
            object IEnumerator.Current => this.current;

            public void Dispose() {
            }
        }
    }
}