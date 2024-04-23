namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    
    public class InvalidGetOperationException : Exception {
        private InvalidGetOperationException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowDisposedEntity(Entity entity) {
            throw new InvalidGetOperationException($"[MORPEH] You are trying to get a component from a disposed entity {entity}");
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowMissing(Entity entity) {
            throw new InvalidGetOperationException($"[MORPEH] You are trying to get a missing component from {entity}");
        }
    }
}