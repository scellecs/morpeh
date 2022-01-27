#if UNITY_2019_1_OR_NEWER
namespace morpeh.Core.NativeCollections {
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using Morpeh.Collections;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.IL2CPP.CompilerServices;

    [NativeContainer]
    public unsafe struct NativeIntHashMap<TNative> : IEnumerable<int> where TNative : unmanaged {
        public int* lengthPtr;
        public int* capacityPtr;
        public int* capacityMinusOnePtr;
        public int* lastIndexPtr;
        public int* freeIndexPtr;
        
        public NativeArray<int>     buckets;
        public NativeArray<Slot>    slots;
        public NativeArray<TNative> data;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() {
            Enumerator e;
            e.hashMap = this;
            e.index   = 0;
            e.current = default;
            return e;
        }
        
        IEnumerator<int> IEnumerable<int>.GetEnumerator() => this.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator : IEnumerator<int> {
            public NativeIntHashMap<TNative> hashMap;

            public int index;
            public int current;

            public bool MoveNext() {
                for (; this.index < *this.hashMap.lastIndexPtr; ++this.index) {
                    var slot = this.hashMap.slots[this.index];
                    if (slot.key - 1 < 0) {
                        continue;
                    }

                    this.current = this.index;
                    ++this.index;

                    return true;
                }

                this.index   = *this.hashMap.lastIndexPtr + 1;
                this.current = default;
                return false;
            }

            public int Current => this.current;

            object IEnumerator.Current => this.current;

            void IEnumerator.Reset() {
                this.index   = 0;
                this.current = default;
            }

            public void Dispose() {
            }
        }
    }
}
#endif