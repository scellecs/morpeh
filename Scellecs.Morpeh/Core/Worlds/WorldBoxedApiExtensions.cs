// TODO: Remove TRUE after migrating functionality to the new API
#if UNITY_EDITOR || MORPEH_ENABLE_RUNTIME_BOXING_API || TRUE

namespace Scellecs.Morpeh {
    using System;
    
    public static class WorldBoxedApiExtensions {
        public static IComponent GetBoxed(this World world, Type type, Entity entity) => world.GetReflectionStash(type).GetBoxed(entity);
        public static IComponent GetBoxed(this World world, Type type, Entity entity, out bool exists) => world.GetReflectionStash(type).GetBoxed(entity, out exists);
        public static void SetBoxed(this World world, Type type, Entity entity, IComponent value) => world.GetReflectionStash(type).SetBoxed(entity, value);
        public static void HasBoxed(this World world, Type type, Entity entity) => world.GetReflectionStash(type).Has(entity);
        public static void RemoveBoxed(this World world, Type type, Entity entity) => world.GetReflectionStash(type).Remove(entity);
        public static void RemoveAllBoxed(this World world, Type type) => world.GetReflectionStash(type).RemoveAll();
        public static void MigrateBoxed(this World world, Type type, Entity from, Entity to) => world.GetReflectionStash(type).Migrate(from, to);
    }
}

#endif