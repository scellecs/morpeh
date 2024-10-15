namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;

    public class ComponentExistsInFilterException : Exception {
        private ComponentExistsInFilterException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Throw(Type value) {
            throw new ComponentExistsInFilterException($"[MORPEH] Component {value} already exists in filter. Check for duplicates or conflicting declarations.");
        }
    }
}