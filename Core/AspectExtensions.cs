namespace Scellecs.Morpeh {
    public static class AspectExtensions {
        public static T Get<T>(this ref AspectFactory<T> aspectDefinition, Entity entity) where T : struct, IAspect {
            var aspect = aspectDefinition.value;
            aspect.Entity = entity;
            return aspect;
        }
    }
}
