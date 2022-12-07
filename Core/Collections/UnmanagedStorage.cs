namespace Scellecs.Morpeh.Collections {
    using System;

    internal struct UnmanagedStorage<T> where T : unmanaged {
        internal IntPtr Ptr;
        internal int Length;
        internal int Capacity;
        internal bool IsCreated;
    }
}