namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Unity.IL2CPP.CompilerServices;

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public unsafe struct PinnedArray<T> : IDisposable where T : unmanaged {
        public T[] data;
        public GCHandle handle;
        public T* ptr;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PinnedArray(int size) {
            this.data = new T[size];
            this.handle = GCHandle.Alloc(this.data, GCHandleType.Pinned);
            this.ptr = (T*)this.handle.AddrOfPinnedObject();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize) {
            this.handle.Free();
            var newArray = new T[newSize];
            Array.Copy(this.data, 0, newArray, 0, this.data.Length);
            this.data = newArray;
            this.handle = GCHandle.Alloc(newArray, GCHandleType.Pinned);
            this.ptr = (T*)this.handle.AddrOfPinnedObject();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            Array.Clear(this.data, 0, this.data.Length);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() {
            if (this.handle.IsAllocated) {
                this.handle.Free();
                this.ptr = (T*)IntPtr.Zero;
                this.data = null;
            }
        }
    }
}
