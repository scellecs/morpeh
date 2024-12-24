#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
namespace Scellecs.Morpeh.WorldBrowser.Filter {
    internal readonly struct EntityHandle : IEquatable<EntityHandle> {
        internal readonly Entity entity;
        internal readonly long archetypeHash;

        internal static EntityHandle Invalid => new EntityHandle(default, default);
        internal World World => this.entity.GetWorld();
        internal bool IsValid => !this.World.IsNullOrDisposed() && !this.World.IsDisposed(this.entity);
        internal Archetype Archetype => this.World.entities[this.entity.Id].currentArchetype;

        public EntityHandle(Entity entity, long archetypeHash) {
            this.entity = entity;
            this.archetypeHash = archetypeHash;
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
