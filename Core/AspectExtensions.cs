namespace Scellecs.Morpeh {
    public static class AspectExtensions {
        public static T Set<T>(this ref Aspect<T> aspectDefinition, Entity entity) where T : struct, IAspect {
            var aspect = aspectDefinition.value;
            aspect.Entity = entity;
            return aspect;
        }
    }
}
