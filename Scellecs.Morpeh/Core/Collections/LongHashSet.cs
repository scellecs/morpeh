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
    public sealed class LongHashSet {
        public int length;
        public int capacity;
        public int capacityMinusOne;
        public int lastIndex;
        public int freeIndex;

        public IntPinnedArray buckets;
        public PinnedArray<long> slots;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LongHashSet() : this(0) {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LongHashSet(int capacity) {
            this.lastIndex = 0;
            this.length    = 0;
            this.freeIndex = -1;

            this.capacityMinusOne = HashHelpers.GetCapacity(capacity);
            this.capacity         = this.capacityMinusOne + 1;
            this.buckets          = new IntPinnedArray(this.capacity);
            this.slots            = new PinnedArray<long>(this.capacity * 2);
        }
    }
}