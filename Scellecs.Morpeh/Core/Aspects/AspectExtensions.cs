using System;

namespace Scellecs.Morpeh {
#if !MORPEH_SUPPRESS_OBSOLETE
    [Obsolete("Will be removed in future versions")]
#endif
    public static class AspectExtensions {
        public static T Get<T>(this ref AspectFactory<T> aspectFactory, Entity entity) where T : struct, IAspect {
            var aspect = aspectFactory.value;
            aspect.Entity = entity;
            return aspect;
        }
    }
}
