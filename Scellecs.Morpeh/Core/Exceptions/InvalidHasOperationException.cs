namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    
    public class InvalidHasOperationException : Exception {
        private InvalidHasOperationException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowDisposedEntity(Entity entity, Type type) {
            throw new InvalidHasOperationException($"[MORPEH] You are trying to check '{type}' against a disposed entity {entity}");
        }
    }
}