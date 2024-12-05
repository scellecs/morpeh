#if DEBUG && !DEVELOPMENT_BUILD
using System.Diagnostics;
namespace Scellecs.Morpeh {
    public sealed class EntityDebuggerProxy {
        private readonly Entity entity;

        public EntityDebuggerProxy(Entity entity) {
            this.entity = entity;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IComponent[] Components => GetComponents();

        private IComponent[] GetComponents() {
            var world = this.entity.GetWorld();
            if (world != null && !world.IsDisposed(this.entity)) {
                var entityData = world.entities[this.entity.Id];
                var archetype = entityData.currentArchetype;
                if (archetype != null) {
                    var componentIds = archetype.components;
                    var components = new IComponent[componentIds.length];
                    var index = 0;
                    foreach (var typeId in componentIds) {
                        var definition = ExtendedComponentId.Get(typeId);
                        var component = definition.entityGetComponentBoxed.Invoke(this.entity);
                        components[index++] = (IComponent)component;
                    }

                    return components;
                }
            }

            return default;
        }
    }
}
#endif