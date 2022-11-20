namespace Morpeh.Collections {
    using System;
    
    internal struct UnsafeArray<T> where T : unmanaged {
        internal IntPtr Ptr;
        internal int Length;
        internal bool IsCreated;
    }
}