namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    
    public class InvalidMigrateOperationException : Exception {
        private InvalidMigrateOperationException(string message) : base(message) { }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowDisposedEntityFrom(Entity entity) {
            throw new InvalidMigrateOperationException($"[MORPEH] You are trying to migrate from a disposed entity {entity}");
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowDisposedEntityTo(Entity entity) {
            throw new InvalidMigrateOperationException($"[MORPEH] You are trying to migrate to a disposed entity {entity}");
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowDisposedEntityFrom(Entity entity, Type type) {
            throw new InvalidMigrateOperationException($"[MORPEH] You are trying to migrate '{type}' from a disposed entity {entity}");
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ThrowDisposedEntityTo(Entity entity, Type type) {
            throw new InvalidMigrateOperationException($"[MORPEH] You are trying to migrate '{type}' to a disposed entity {entity}");
        }
    }
}