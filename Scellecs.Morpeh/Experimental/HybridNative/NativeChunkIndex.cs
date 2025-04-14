using System.Runtime.CompilerServices;
using Unity.Entities;

namespace Scellecs.Morpeh.Experimental {
    internal unsafe struct NativeChunkIndex {
        internal static NativeChunkIndex Null => new();

        private int value;

        public NativeChunkIndex(int value) => this.value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NativeChunk* ChunkPtr() => EntityComponentStore.chunkStore.Data.GetChunkPointer(value);

        public bool Equals(NativeChunkIndex other) => value == other.value;
        public override bool Equals(object obj) => obj is NativeChunkIndex other && Equals(other);
        public override int GetHashCode() => value;

        public static implicit operator int(NativeChunkIndex index) => index.value;
        public static bool operator ==(NativeChunkIndex a, NativeChunkIndex b) => a.value == b.value;
        public static bool operator !=(NativeChunkIndex a, NativeChunkIndex b) => !(a == b);
        public static bool operator <(NativeChunkIndex a, NativeChunkIndex b) => a.value < b.value;
        public static bool operator >(NativeChunkIndex a, NativeChunkIndex b) => a.value > b.value;
    }
}
