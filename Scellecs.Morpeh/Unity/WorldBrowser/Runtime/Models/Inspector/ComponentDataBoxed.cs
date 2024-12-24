#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

namespace Scellecs.Morpeh.WorldBrowser {
    [Serializable]
    internal struct ComponentDataBoxed {
        public object data;
        public bool isMarker;
        public int typeId;
    }
}
#endif