namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    
    public class InvalidRemoveOperationException : Exception {
        private InvalidRemoveOperationException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowDisposedEntity(Entity entity, Type type) {
            throw new InvalidRemoveOperationException($"[MORPEH] You are trying to remove '{type}' from a disposed entity {entity}");
        }
    }
}