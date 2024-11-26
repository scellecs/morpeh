namespace Scellecs.Morpeh.Collections {
    using System;

    internal struct UnmanagedStorage {
        internal IntPtr Ptr;
        internal int Length;
        internal int Capacity;
        internal bool IsCreated;
    }
}