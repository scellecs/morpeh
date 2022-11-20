namespace Morpeh.Collections {
    using System;
    
    internal struct UnsafeStorage<T> where T : unmanaged{
        internal IntPtr Ptr;
        internal int Length;
        internal int Capacity;
        internal bool IsCreated;
    }
}