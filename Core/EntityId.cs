namespace Scellecs.Morpeh {
    using System;
    using Unity.IL2CPP.CompilerServices;

#if UNITY_EDITOR && ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

#if !MORPEH_NON_SERIALIZED
    [Serializable]
#endif
    [Il2CppEagerStaticClassConstruction]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct EntityId : IEquatable<EntityId> {
        internal static readonly EntityId Default = new EntityId(-1, -1);
#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        private int Id => this.id;
#endif
        internal readonly int id;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [ShowInInspector]
        private int Gen => this.gen;
#endif
        internal readonly int gen;


        public EntityId(int id, int gen) {
            this.id  = id;
            this.gen = gen;
        }

        public bool Equals(EntityId other) {
            return this.id == other.id && this.gen == other.gen;
        }

        public override bool Equals(object obj) {
            return obj is EntityId other && this.Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.id * 397) ^ this.gen;
            }
        }

        public static bool operator ==(EntityId left, EntityId right) {
            return left.Equals(right);
        }

        public static bool operator !=(EntityId left, EntityId right) {
            return !(left == right);
        }

        public override string ToString() {
            return $"EntityId(id={this.id.ToString()}, gen={this.gen.ToString()})";
        }
        
        public int CompareTo(EntityId other) {
            return this.id.CompareTo(other.id);
        }

        public static EntityId Invalid => new EntityId(-1, -1);
    }
}
