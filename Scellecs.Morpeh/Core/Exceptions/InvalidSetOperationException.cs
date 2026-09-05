namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    
    public class InvalidSetOperationException : Exception {
        private InvalidSetOperationException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowDisposedEntity(Entity entity, Type type) {
            throw new InvalidSetOperationException($"[MORPEH] You are trying to set '{type}' to a disposed entity {entity}");
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowInvalidComponentType(Entity entity, Type expectedType, Type actualType) {
            throw new InvalidSetOperationException($"[MORPEH] You are trying to set '{actualType}' to '{expectedType}' component of entity {entity}");
        }
    }
}