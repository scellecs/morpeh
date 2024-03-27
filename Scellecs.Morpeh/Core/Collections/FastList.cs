namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FastList<T> {
        public T[] data;
        public int length;
        public int capacity;

        public EqualityComparer<T> comparer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastList() {
            this.capacity = 4;
            this.data     = new T[this.capacity];
            this.length   = 0;

            this.comparer = EqualityComparer<T>.Default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FastList(int capacity) {
            this.capacity = HashHelpers.GetCapacity(capacity) + 1;
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
            
            e.data = this.data;
            e.index = -1;
            e.length = this.length;
            
            return e;
        }

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
        public struct Enumerator {
            public T[] data;
            public int index;
            public int length;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() {
                return ++this.index < this.length;
            }

            public T Current {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.data[this.index];
            }
        }
    }
}