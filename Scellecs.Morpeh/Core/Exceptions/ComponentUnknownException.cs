namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    
    public class ComponentUnknownException : Exception {
        private ComponentUnknownException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowTypeNotFound(Type value) {
            throw new ComponentUnknownException($"Type {value} not found. Use ExtendedComponentId.Get<T>() once before trying to get via type.");
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowTypeIdNotFound(int value) {
            throw new ComponentUnknownException($"Type with id {value} not found. Use ExtendedComponentId.Get<T>() once before trying to get via type id.");
        }
    }
}