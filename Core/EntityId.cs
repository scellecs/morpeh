using System;

namespace Morpeh
{
    public readonly struct EntityId : IEquatable<EntityId> {
        public readonly int internalId;
        public readonly int internalGen;
        
        public EntityId(int internalId, int internalGen) {
            this.internalId = internalId;
            this.internalGen = internalGen;
        }

        public bool Equals(EntityId other) {
            return internalId == other.internalId && internalGen == other.internalGen;
        }

        public override bool Equals(object obj) {
            return obj is EntityId other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(internalId, internalGen);
        }
        
        public static bool operator ==(EntityId left, EntityId right) {
            return left.Equals(right);
        }

        public static bool operator !=(EntityId left, EntityId right) {
            return !(left == right);
        }
        
        public override string ToString() {
            return $"EntityId(id={internalId}, gen={internalGen})";
        }
        
        public static EntityId Invalid => new EntityId(-1, -1);
    }
}