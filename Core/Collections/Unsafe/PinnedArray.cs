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

    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public unsafe struct PinnedArray<T> : IDisposable, IEnumerable<T> where T : unmanaged {
        public T[] value;
        public T* ptr;
#if MORPEH_UNITY
        public ulong handle;
#else
        public GCHandle handle;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PinnedArray(int size) {
            this.value = new T[size];
#if MORPEH_UNITY
            this.ptr = (T*) UnsafeUtility.PinGCArrayAndGetDataAddress(this.value, out this.handle);
#else
            this.handle = GCHandle.Alloc(this.value, GCHandleType.Pinned);
            this.ptr = (T*)this.handle.AddrOfPinnedObject();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Resize(int newSize) {
#if MORPEH_UNITY
            UnsafeUtility.ReleaseGCObject(this.handle);
#else
            this.handle.Free();
#endif
            var newArray = new T[newSize];
            var len = this.value.Length;
            Array.Copy(this.value, 0, newArray, 0, newSize >= len ? len : newSize);
            this.value = newArray;
#if MORPEH_UNITY
            this.ptr = (T*) UnsafeUtility.PinGCArrayAndGetDataAddress(this.value, out this.handle);
#else
            this.handle = GCHandle.Alloc(newArray, GCHandleType.Pinned);
            this.ptr = (T*)this.handle.AddrOfPinnedObject();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            Array.Clear(this.value, 0, this.value.Length);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() {
#if MORPEH_UNITY
            UnsafeUtility.ReleaseGCObject(this.handle);
            this.ptr = (T*)IntPtr.Zero;
            this.value = null;
#else
            if (this.handle.IsAllocated) {
                this.handle.Free();
                this.ptr = (T*)IntPtr.Zero;
                this.value = null;
            }
#endif
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.length = this.value.Length;
            e.ptr    = this.ptr;
            e.current = default;
            e.index   = 0;
            return e;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator : IEnumerator<T> {
            public T* ptr;

            public int length;
            public T   current;
            public int index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() {
                if (this.index >= this.length) {
                    return false;
                }

                this.current = this.ptr[this.index++];

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
