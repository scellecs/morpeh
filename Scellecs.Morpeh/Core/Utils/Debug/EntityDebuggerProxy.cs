#if DEBUG && !DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
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
                var addedComponents = entityData.addedComponents;
                var removedComponents = entityData.removedComponents;
                var removedComponentsCount = entityData.removedComponentsCount;
                var components = new List<IComponent>();

                for (int i = 0; i < entityData.addedComponentsCount; i++) {
                    var typeId = addedComponents[i];
                    var definition = ExtendedComponentId.Get(typeId);
                    components.Add((IComponent)definition.entityGetComponentBoxed.Invoke(this.entity));
                }

                if (archetype != null) {
                    var componentIds = archetype.components;
                    foreach (var typeId in componentIds) {
                        var isRemoved = false;
                        for (int i = 0; i < removedComponentsCount; i++) {
                            if (removedComponents[i] == typeId) {
                                isRemoved = true;
                                break;
                            }
                        }

                        if (!isRemoved) {
                            var definition = ExtendedComponentId.Get(typeId);
                            components.Add((IComponent)definition.entityGetComponentBoxed.Invoke(this.entity));
                        }
                    }
                }

                return components.ToArray();
            }

            return default;
        }
    }
}
#endif