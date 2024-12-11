namespace Scellecs.Morpeh.Collections {
    using System;
    using System.Runtime.CompilerServices;
    using Unity.IL2CPP.CompilerServices;
    
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class BitSet {
        private ulong[] data;
        internal int longsCapacity;
        
        public BitSet(int capacity = 64) {
            this.longsCapacity = GetMinLengthForCapacity(capacity);
            this.data = new ulong[this.longsCapacity];
        }
        
        public BitSet(int[] setBits) {
            var max = 0;
            for (var i = 0; i < setBits.Length; i++) {
                if (setBits[i] > max) {
                    max = setBits[i];
                }
            }
            
            this.longsCapacity = GetMinLengthForCapacity(max + 1);
            this.data = new ulong[this.longsCapacity];
            
            for (var i = 0; i < setBits.Length; i++) {
                this.Set(setBits[i]);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMinLengthForCapacity(int capacity) {
            return (capacity >> 6) + 1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Set(int index) {
            var arrayIndex = index >> 6;
            var bitIndex = index & 63;
            
            if (arrayIndex >= this.longsCapacity) {
                this.ExpandTo(arrayIndex);
            }
            
            var mask = 1UL << bitIndex;
            var value = this.data[arrayIndex];
            var result = (value & mask) == 0;
            
            this.data[arrayIndex] = value | mask;
            
            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Unset(int index) {
            var arrayIndex = index >> 6;
            
            if (arrayIndex >= this.longsCapacity) {
                return false;
            }
            
            var bitIndex = index & 63;
            var mask = 1UL << bitIndex;
            var value = this.data[arrayIndex];
            var result = (value & mask) != 0;
            
            this.data[arrayIndex] = value & ~mask;
            
            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSet(int index) {
            var arrayIndex = index >> 6;
            return arrayIndex < this.longsCapacity && (this.data[arrayIndex] & (1UL << (index & 63))) != 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ValueOf(int index) {
            var arrayIndex = index >> 6;
            return arrayIndex < this.longsCapacity ? (int) ((this.data[arrayIndex] >> (index & 63)) & 1) : 0;
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void ExpandTo(int arrayIndex) {
            var newSize = arrayIndex + 1;
            ArrayHelpers.Grow(ref this.data, newSize);
            this.longsCapacity = newSize;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            Array.Clear(this.data, 0, this.longsCapacity);
        }
    }
}