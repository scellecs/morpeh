namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    
    public class FilterIndexOutOfRangeException : Exception {
        private FilterIndexOutOfRangeException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Throw(int value) {
            throw new FilterIndexOutOfRangeException($"[MORPEH] Filter index out of range {value}");
        }
    }
}