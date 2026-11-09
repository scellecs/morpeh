namespace Scellecs.Morpeh {
    using System;
    using Sirenix.OdinInspector;
    using Unity.IL2CPP.CompilerServices;
    using System.Runtime.CompilerServices;
    using System.Diagnostics;

#if UNITY_EDITOR || DEBUG && !ENABLE_MONO && !ENABLE_IL2CPP
    [DebuggerTypeProxy(typeof(EntityDebuggerProxy))]
#endif
    [Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct Entity : IEquatable<Entity> {
        internal readonly long value;

        // [ Generation: 16 bits | WorldId: 8 bits | WorldGeneration: 8 bits | Id: 32 bits ]
        internal Entity(int worldId, int worldGeneration, int id, ushort generation) {
            value = ((generation & 0xFFFFL) << 48) | ((worldId & 0xFFL) << 40) | ((worldGeneration & 0xFFL) << 32) | (id & 0xFFFFFFFFL);
        }

        [ShowInInspector]
        public int Id {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)this.value;
        }

        [ShowInInspector]
        public ushort Generation {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (ushort)(this.value >> 48);
        }

        [ShowInInspector]
        public int WorldId {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)((this.value >> 40) & 0xFF);
        }

        [ShowInInspector]
        public int WorldGeneration {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)((this.value >> 32) & 0xFF);
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
        
        public int CompareTo(Entity other) {
            return this.Id.CompareTo(other.Id);
        }

        public override string ToString() {
            return $"Entity: Id={this.Id}, Generation={this.Generation}, WorldId={this.WorldId}, WorldGeneration={this.WorldGeneration}";
        }
    }
}
