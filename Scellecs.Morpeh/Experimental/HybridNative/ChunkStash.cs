#if ENABLE_MONO || ENABLE_IL2CPP
#define MORPEH_UNITY
#endif

#if MORPEH_UNITY
using JetBrains.Annotations;
using System;
using System.Runtime.CompilerServices;

namespace Scellecs.Morpeh.Experimental {
    public unsafe sealed class ChunkStash<T> : IStash where T : unmanaged, IChunkComponent {
        internal World world;
        private TypeInfo typeInfo;
        private ChunkComponentTypeInfo typeInfoNative;

        internal T empty;
        private Type type;

        [PublicAPI]
        public bool IsDisposed;

        [PublicAPI]
        public Type Type {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.type;
        }

        [PublicAPI]
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => -1;
        }

        [UnityEngine.Scripting.Preserve]
        internal ChunkStash(World world, TypeInfo typeInfo, ChunkComponentTypeInfo typeInfoNative, int capacity = -1)
        {
            this.world = world;
            this.typeInfo = typeInfo;
            this.typeInfoNative = typeInfoNative;
            this.empty = default;
            this.type = typeof(T);
        }

        public void Set(Entity entity)
        {

            throw new NotImplementedException();
        }

        public bool Remove(Entity entity)
        {
            throw new NotImplementedException();
        }

        public void RemoveAll()
        {
            throw new NotImplementedException();
        }

        public void Migrate(Entity from, Entity to, bool overwrite = true)
        {
            throw new NotImplementedException();
        }

        public bool Has(Entity entity)
        {
            throw new NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IStash.Clean(Entity entity)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
#endif
