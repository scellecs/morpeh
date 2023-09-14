namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class IntStack {
        public int length;
        public int capacity;

        public IntPinnedArray data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntStack() {
            this.capacity = 4;
            this.data     = new IntPinnedArray(this.capacity);
            this.length   = 0;
        }

        ~IntStack() {
            this.data.Dispose();
        }
    }
}