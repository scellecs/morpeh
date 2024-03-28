namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class BitSet {
        private ulong[] data;
        internal int setBitsCount;
        internal int longsCapacity;
        
        public BitSet(int capacity = 64) {
            this.longsCapacity = GetMinLengthForCapacity(capacity);
            this.data = new ulong[this.longsCapacity];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMinLengthForCapacity(int capacity) {
            return (capacity >> 6) + 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(int index) {
            var arrayIndex = index >> 6;
            var bitIndex   = index & 63;
            
            if (arrayIndex >= this.longsCapacity) {
                var newSize = arrayIndex + 1;
                Array.Resize(ref this.data, newSize);
                this.longsCapacity = newSize;
            }
            
            var mask = 1UL << bitIndex;
            var value = this.data[arrayIndex];
            var result = (value & mask) == 0;
            
            this.setBitsCount += result ? 1 : 0;
            this.data[arrayIndex] = value | mask;
            
            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unset(int index) {
            var arrayIndex = index >> 6;
            
            if (arrayIndex >= this.longsCapacity) {
                return false;
            }
            
            var bitIndex   = index & 63;
            var mask = 1UL << bitIndex;
            var value = this.data[arrayIndex];
            var result = (value & mask) != 0;
            
            this.setBitsCount -= result ? 1 : 0;
            this.data[arrayIndex] = value & ~mask;
            
            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(int index) {
            var arrayIndex = index >> 6;
            var bitIndex = index & 63;
            
            if (arrayIndex >= this.longsCapacity) {
                return false;
            }
            
            var mask = 1UL << bitIndex;
            var value = this.data[arrayIndex];
            
            return (value & mask) != 0;
        }
        
        public void Clear() {
            Array.Clear(this.data, 0, this.longsCapacity);
            this.setBitsCount = 0;
        }
        
        public Enumerator GetEnumerator() {
            return new Enumerator {
                data = this.data,
                longIndex = 0,
                bitIndex = 0,
            };
        }
        
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        public struct Enumerator {
            internal ulong[] data;
            internal int longIndex;
            internal int bitIndex;

            public bool MoveNext() {
                var length = this.data.Length;

                while (this.longIndex < length) {
                    var value = this.data[this.longIndex];
                    
                    while (this.bitIndex < 64) {
                        if ((value & (1UL << this.bitIndex)) != 0) {
                            this.bitIndex++;
                            return true;
                        }
                        this.bitIndex++;
                    }
                    
                    this.longIndex++;
                    this.bitIndex = 0;
                }
                
                return false;
            }

            public int Current {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => (this.longIndex << 6) + this.bitIndex - 1;
            }
        }
    }
}