#if (ENABLE_MONO || ENABLE_IL2CPP) //Unity Engine
    #if UNITY_2020_1_OR_NEWER 
        #define MORPEH_UNMANAGED
    #endif
#else // pure .Net
    #define MORPEH_UNMANAGED
#endif

#if MORPEH_UNMANAGED
namespace Scellecs.Morpeh.Collections {
    using System;

    internal struct UnmanagedStorage<T> where T : unmanaged {
        internal IntPtr Ptr;
        internal int Length;
        internal int Capacity;
        internal bool IsCreated;
    }
}
#endif