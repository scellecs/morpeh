namespace Scellecs.Morpeh.Experimental {
    internal struct NativeChunkConstants {
        internal const int MAX_CHUNK_CAPACITY = 128;
        internal const int CHUNK_BUFFER_OFFSET = 64;
        internal const int CHUNK_SIZE = 16 * 1024;
        internal const int CHUNK_BUFFER_SIZE = CHUNK_SIZE - CHUNK_BUFFER_OFFSET;
    }
}
