namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    
    public class InvalidSetOperationException : Exception {
        private InvalidSetOperationException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowDisposedEntity(Entity entity, Type type) {
            throw new InvalidSetOperationException($"[MORPEH] You are trying to set '{type}' to a disposed entity {entity}");
        }
    }
}