using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh {
    using System;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    
#if !MORPEH_NON_SERIALIZED
    [Serializable]
#endif
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public partial struct Entity : IEquatable<Entity> {
        public static Entity Invalid => default;
        
        internal long value;
        
        internal Entity(int worldId, int id, int generation) {
            value = ((id & 0xFFFFFFFFL) << 32) | ((generation & 0xFFFFFFL) << 8) | (worldId & 0xFFL);
        }

        [ShowInInspector]
        public int Id {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)((value >> 32) & 0xFFFFFFFF);
        }

        [ShowInInspector]
        public int Generation {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)((value >> 8) & 0xFFFFFF);
        }

        [ShowInInspector]
        public int WorldId {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)(value & 0xFF);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Entity lhs, Entity rhs) {
            return lhs.value == rhs.value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Entity lhs, Entity rhs) {
            return lhs.value != rhs.value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Entity other) {
            return this.value == other.value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) {
            return obj is Entity other && this.Equals(other);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() {
            return this.value.GetHashCode();
        }

        public override string ToString() {
            return $"Entity: Id={this.Id}, Generation=({this.Generation}, WorldId={this.WorldId})";
        }
    }
}
