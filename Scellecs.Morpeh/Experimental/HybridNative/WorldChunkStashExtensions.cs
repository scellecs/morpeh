namespace Scellecs.Morpeh.Experimental {
    public static class WorldChunkStashExtensions {
        public static ChunkStash<T> GetChunkStash<T>(this World world) where T : unmanaged, IChunkComponent {
            world.ThreadSafetyCheck();

            var info = ComponentId<T>.info;
            var nativeInfo = ComponentId<T>.infoNative;
            var candidate = world.GetExistingStash(info.id);

            if (candidate != null) {
                return (ChunkStash<T>)candidate;
            }

            world.EnsureStashCapacity(info.id);
            var capacity = ComponentId<T>.StashSize;
            var stash = new ChunkStash<T>(world, info, nativeInfo, capacity);
            world.stashes[info.id] = stash;
            return stash;
        }
    }
}
