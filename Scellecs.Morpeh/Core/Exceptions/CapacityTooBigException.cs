namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    
    public class CapacityTooBigException : Exception {
        private CapacityTooBigException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Throw(int value) {
            throw new CapacityTooBigException($"[MORPEH] Capacity is too big for min value {value}");
        }
    }
}