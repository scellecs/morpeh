namespace Scellecs.Morpeh.Collections {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    /// <summary>
    /// Similar to HashSet<int></int>. Fully supports only positive range of keys.
    /// </summary>
    public sealed class IntHashSet {
        public int length;
        public int capacity;
        public int capacityMinusOne;
        public int lastIndex;
        public int freeIndex;

        public int[] buckets;
        public int[] slots;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntHashSet() : this(0) {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntHashSet(int capacity) {
            this.lastIndex = 0;
            this.length    = 0;
            this.freeIndex = -1;

            this.capacityMinusOne = HashHelpers.GetCapacity(capacity);
            this.capacity = this.capacityMinusOne + 1;
            this.buckets = new int[this.capacity];
            this.slots = new int[this.capacity * 2];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.set     = this;
            e.index   = 0;
            e.current = default;
            return e;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator {
            public IntHashSet set;

            public int index;
            public int current;

            public bool MoveNext() {
                var len = this.set.lastIndex;
                while (this.index < len) {
                    var v = this.set.slots[this.index] - 1;
                    this.index += 2;
                    
                    if (v < 0) {
                        continue;
                    }
                    
                    this.current = v;
                    return true;
                }

                this.index = len + 1;
                this.current = default;
                return false;
            }

            public int Current {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.current;
            }
        }
    }
}