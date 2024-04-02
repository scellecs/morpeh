#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
#if MORPEH_UNITY
    using Unity.Collections.LowLevel.Unsafe;
#else
    using System.Runtime.InteropServices;
#endif
    using Unity.IL2CPP.CompilerServices;

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public unsafe struct PinnedArray<T> : IDisposable where T : unmanaged {
        public T[] data;
        public T* ptr;
#if MORPEH_UNITY
        public ulong handle;
#else
        public GCHandle handle;
#endif
        
        public T this[int index] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.data[index];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this.data[index] = value;
        }
        
        public int Length => this.data.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PinnedArray(int size) {
            this.data = new T[size];
#if MORPEH_UNITY
            this.ptr = (T*) UnsafeUtility.PinGCArrayAndGetDataAddress(this.data, out this.handle);
#else
            this.handle = GCHandle.Alloc(this.data, GCHandleType.Pinned);
            this.ptr = (T*)this.handle.AddrOfPinnedObject();
#endif
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Resize(int newSize) {
#if MORPEH_UNITY
            UnsafeUtility.ReleaseGCObject(this.handle);
#else
            this.handle.Free();
#endif
            var newArray = new T[newSize];
            var len = this.data.Length;
            Array.Copy(this.data, 0, newArray, 0, newSize >= len ? len : newSize);
            this.data = newArray;
#if MORPEH_UNITY
            this.ptr = (T*) UnsafeUtility.PinGCArrayAndGetDataAddress(this.data, out this.handle);
#else
            this.handle = GCHandle.Alloc(newArray, GCHandleType.Pinned);
            this.ptr = (T*)this.handle.AddrOfPinnedObject();
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
            this.ptr = (T*)IntPtr.Zero;
            this.data = null;
#else
            if (this.handle.IsAllocated) {
                this.handle.Free();
                this.ptr = (T*)IntPtr.Zero;
                this.data = null;
            }
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.index  = -1;
            e.length = this.data.Length;
            e.ptr = this.ptr;
            return e;
        }
        
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator {
            public int index;
            public int length;
            public T* ptr;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() {
                return ++this.index < this.length;
            }

            public T Current {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => this.ptr[this.index];
            }
        }
    }
}
