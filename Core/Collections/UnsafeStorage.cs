namespace Morpeh.Collections {
    using System;
    
    internal struct UnsafeStorage {
        internal IntPtr Ptr;
        internal int Length;
        internal bool IsCreated;
    }
}