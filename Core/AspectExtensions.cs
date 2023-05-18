namespace Scellecs.Morpeh {
    public static class AspectExtensions {
        public static T Get<T>(this ref AspectFactory<T> aspectFactory, Entity entity) where T : struct, IAspect {
            var aspect = aspectFactory.value;
            aspect.Entity = entity;
            return aspect;
        }
    }
}
