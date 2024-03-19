#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
#if MORPEH_UNITY
    using Unity.Collections.LowLevel.Unsafe;
#else
    using System.Runtime.InteropServices;
#endif
    using Unity.IL2CPP.CompilerServices;

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public unsafe struct IntPinnedArray : IDisposable {
        public int[] data;
        public int* ptr;
#if MORPEH_UNITY
        public ulong handle;
#else
        public GCHandle handle;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPinnedArray(int size) {
            this.data = new int[size];
#if MORPEH_UNITY
            this.ptr = (int*) UnsafeUtility.PinGCArrayAndGetDataAddress(this.data, out this.handle);
#else
            this.handle = GCHandle.Alloc(this.data, GCHandleType.Pinned);
            this.ptr = (int*)this.handle.AddrOfPinnedObject();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize) {
#if MORPEH_UNITY
            UnsafeUtility.ReleaseGCObject(this.handle);
#else
            this.handle.Free();
#endif
            var newArray = new int[newSize];
            var len = this.data.Length;
            Array.Copy(this.data, 0, newArray, 0, newSize >= len ? len : newSize);
            this.data = newArray;
#if MORPEH_UNITY
            this.ptr = (int*) UnsafeUtility.PinGCArrayAndGetDataAddress(this.data, out this.handle);
#else
            this.handle = GCHandle.Alloc(newArray, GCHandleType.Pinned);
            this.ptr = (int*)this.handle.AddrOfPinnedObject();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            Array.Clear(this.data, 0, this.data.Length);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() {
#if MORPEH_UNITY
            UnsafeUtility.ReleaseGCObject(this.handle);
            this.ptr = (int*)IntPtr.Zero;
            this.data = null;
#else
            if (this.handle.IsAllocated) {
                this.handle.Free();
                this.ptr = (int*)IntPtr.Zero;
                this.data = null;
            }
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.lengthMinusOne = this.data.Length - 1;
            e.ptr    = this.ptr;
            e.index   = -1;
            return e;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator {
            public int* ptr;

            public int lengthMinusOne;
            public int index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() {
                return ++this.index <= this.lengthMinusOne;
            }
            
            public int Current => this.ptr[this.index];
        }
    }
}
