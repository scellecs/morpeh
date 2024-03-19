using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Scellecs.Morpeh.Collections
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class BitSet {
        private PinnedArray<ulong> data;
        internal int length;
        
        public BitSet(int capacity = 64) {
            this.data = new PinnedArray<ulong>(GetMinLengthForCapacity(capacity));
        }
        
        ~BitSet() {
            this.data.Dispose();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMinLengthForCapacity(int capacity) {
            return (capacity >> 6) + 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(int index) {
            var arrayIndex = index >> 6;
            var bitIndex   = index & 63;
            
            if (arrayIndex >= this.data.Length) {
                this.Resize(arrayIndex + 1);
            }
            
            var mask = 1UL << bitIndex;
            var value = this.data[arrayIndex];
            var result = (value & mask) == 0;
            
            this.length += result ? 1 : 0;
            this.data[arrayIndex] = value | mask;
            
            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unset(int index) {
            var arrayIndex = index >> 6;
            
            if (arrayIndex >= this.data.Length) {
                return false;
            }
            
            var bitIndex   = index & 63;
            var mask = 1UL << bitIndex;
            var value = this.data[arrayIndex];
            var result = (value & mask) != 0;
            
            this.length -= result ? 1 : 0;
            this.data[arrayIndex] = value & ~mask;
            
            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(int index) {
            var arrayIndex = index >> 6;
            var bitIndex   = index & 63;
            
            if (arrayIndex >= this.data.Length) {
                return false;
            }
            
            var mask = 1UL << bitIndex;
            var value = this.data[arrayIndex];
            
            return (value & mask) != 0;
        }
        
        public void Clear() {
            this.data.Clear();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Resize(int newSize) {
            this.data.Resize(newSize);
        }
        
        public Enumerator GetEnumerator() {
            return new Enumerator {
                bitSet = this,
            };
        }
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator {
            public BitSet bitSet;

            public int longIndex;
            public int bitIndex;
            private int current;

            public bool MoveNext() {
                var data = this.bitSet.data;
                var length = data.Length;
                while (this.longIndex < length) {
                    var value = data[this.longIndex];
                    if (value != 0) {
                        while (this.bitIndex < 64) {
                            if ((value & (1UL << this.bitIndex)) != 0) {
                                this.current = (this.longIndex << 6) + this.bitIndex;
                                this.bitIndex++;
                                return true;
                            }
                            this.bitIndex++;
                        }
                    }
                    this.longIndex++;
                    this.bitIndex = 0;
                }
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