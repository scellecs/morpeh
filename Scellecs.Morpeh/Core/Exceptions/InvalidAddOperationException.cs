namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    
    public class InvalidAddOperationException : Exception {
        private InvalidAddOperationException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowDisposedEntity(Entity entity, Type type) {
            throw new InvalidAddOperationException($"[MORPEH] You are trying to add '{type}' to a disposed entity {entity}");
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowAlreadyExists(Entity entity, Type type) {
            throw new InvalidAddOperationException($"[MORPEH] You are trying to add already existing '{type}' to {entity}");
        }
    }
}