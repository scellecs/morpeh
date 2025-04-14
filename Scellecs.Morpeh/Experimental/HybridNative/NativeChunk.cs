using System.Runtime.InteropServices;

namespace Scellecs.Morpeh.Experimental {
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct NativeChunk {
        [FieldOffset(NativeChunkConstants.CHUNK_BUFFER_OFFSET)]
        internal fixed byte buffer[4];
    }
}
