namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    
    public class FilterSourceSequenceIsEmptyException : Exception {
        private FilterSourceSequenceIsEmptyException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Throw() {
            throw new FilterSourceSequenceIsEmptyException($"[MORPEH] Filter source sequence is empty");
        }
    }
}