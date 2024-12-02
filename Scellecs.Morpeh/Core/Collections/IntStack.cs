namespace Scellecs.Morpeh.Collections {
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class IntStack {
        public int length;
        public int capacity;

        public int[] data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntStack() {
            this.capacity = 4;
            this.data = new int[this.capacity];
            this.length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntStack(int capacity) {
            this.capacity = capacity;
            this.data = new int[this.capacity];
            this.length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(in int value) {
            if (this.length == this.capacity) {
                this.Expand();
            }

            this.data[this.length++] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Pop() {
            return this.data[--this.length];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out int value) {
            if (this.length == 0) {
                value = default;
                return false;
            }

            value = this.data[--this.length];
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void Expand() {
            this.capacity *= 2;
            ArrayHelpers.Grow(ref this.data, this.capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            this.length = 0;
        }
    }
}