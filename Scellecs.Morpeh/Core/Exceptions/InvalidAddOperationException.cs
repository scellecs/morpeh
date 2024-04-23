namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    
    public class InvalidAddOperationException : Exception {
        private InvalidAddOperationException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowDisposedEntity(Entity entity) {
            throw new InvalidAddOperationException($"[MORPEH] You are trying to add a component to a disposed entity {entity}");
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowAlreadyExists(Entity entity) {
            throw new InvalidAddOperationException($"[MORPEH] You are trying to add an already existing component to {entity}");
        }
    }
}