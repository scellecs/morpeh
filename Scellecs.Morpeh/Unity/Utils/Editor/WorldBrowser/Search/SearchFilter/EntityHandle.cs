#if UNITY_EDITOR
using System;
namespace Scellecs.Morpeh.Utils.Editor {
    internal readonly struct EntityHandle : IEquatable<EntityHandle> {
        internal readonly Entity entity;
        internal readonly long archetypeHash;

        internal World World => this.entity.GetWorld();
        internal bool IsValid => !this.World.IsNullOrDisposed() && !this.World.IsDisposed(this.entity);
        internal Archetype Archetype => this.World.entities[this.entity.Id].currentArchetype;

        public EntityHandle(Entity entity, long archetypeHash) {
            this.entity = entity;
            this.archetypeHash = archetypeHash;
        }

        public bool ArchetypesEqual(EntityHandle other) {
            return this.archetypeHash.Equals(other.archetypeHash);
        }

        public bool EntitiesEqual(EntityHandle other) {
            return this.entity.Equals(other.entity);
        }

        public bool Equals(EntityHandle other) {
            return this.entity.Equals(other.entity) && this.archetypeHash.Equals(other.archetypeHash);
        }
    }
}
#endif
