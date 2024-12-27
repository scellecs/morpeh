#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
namespace Scellecs.Morpeh.WorldBrowser.Filter {
    internal readonly struct EntityHandle : IEquatable<EntityHandle> {
        internal readonly Entity entity;
        internal readonly long archetypeHash;

        internal World World => this.entity.GetWorld();
        internal Archetype Archetype => this.World.entities[this.entity.Id].currentArchetype;
        internal EntityData EntityData => this.World.entities[entity.Id];
        internal bool IsValid => !this.World.IsNullOrDisposed() && !this.World.IsDisposed(this.entity);

        public EntityHandle(Entity entity) {
            var world = entity.GetWorld();
            if (!world.IsNullOrDisposed() && !world.IsDisposed(entity)) {
                this.entity = entity;
                this.archetypeHash = world.entities[entity.Id].currentArchetype.hash.GetValue();
            }
            else {
                this.entity = default;
                this.archetypeHash = default;
            }
        }

        public bool Equals(EntityHandle other) {
            return this.entity.Equals(other.entity) && this.archetypeHash.Equals(other.archetypeHash);
        }

        public override string ToString() {
            return $"{entity.Id}:{entity.Generation}, IsValid:{IsValid}, archetypeHash:{archetypeHash}";
        }
    }
}
#endif
