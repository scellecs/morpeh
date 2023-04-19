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
    public sealed unsafe class UnsafeFastList<T> where T : unmanaged {
        public PinnedArray<T> data;
        public int length;
        public int capacity;
        public int lastSwappedIndex;

        public EqualityComparer<T> comparer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFastList() {
            this.capacity = 3;
            this.data     = new PinnedArray<T>(this.capacity);
            this.length   = 0;
            this.lastSwappedIndex = -1;

            this.comparer = EqualityComparer<T>.Default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFastList(int capacity) {
            this.capacity = HashHelpers.GetCapacity(capacity);
            this.data     = new PinnedArray<T>(this.capacity);
            this.length   = 0;
            this.lastSwappedIndex = -1;

            this.comparer = EqualityComparer<T>.Default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeFastList(UnsafeFastList<T> other) {
            this.capacity = other.capacity;
            this.data     = new PinnedArray<T>(this.capacity);
            this.length   = other.length;
            this.lastSwappedIndex = -1;
            Array.Copy(other.data.data, 0, this.data.data, 0, this.length);

            this.comparer = other.comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.length = this.length;
            e.list    = this;
            e.current = default;
            e.index   = 0;
            this.lastSwappedIndex = -1;
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
            public UnsafeFastList<T> list;

            public int length;
            public T   current;
            public int index;

            public bool MoveNext() {
                var lastSwappedIndex = this.list.lastSwappedIndex;
                if (lastSwappedIndex != -1) {
                    this.length = this.list.length;
                    var previousIndex = this.index - 1;
                    if (lastSwappedIndex == previousIndex) {
                        this.index--;
                    }
                    else if (lastSwappedIndex < previousIndex) {
#if MORPEH_DEBUG
                        throw new InvalidOperationException("Earlier collection items have been modified, this is not allowed");
#endif
                    }
                }
                
                if (this.index >= this.length) {
                    return false;
                }

                this.current = this.list.data.ptr[this.index++];
                this.list.lastSwappedIndex = -1;

                return true;
            }

            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.current;
            }
        }
    }
}
