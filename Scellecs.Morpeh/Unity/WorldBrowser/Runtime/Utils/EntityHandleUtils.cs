#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser.Filter {
    internal static class EntityHandleUtils {
        public static bool IsValid(Entity entity, World world) => !world.IsNullOrDisposed() && !world.IsDisposed(entity);
        public static Archetype GetArchetype(Entity entity) => entity.GetWorld().entities[entity.Id].currentArchetype;
    }
}
#endif
