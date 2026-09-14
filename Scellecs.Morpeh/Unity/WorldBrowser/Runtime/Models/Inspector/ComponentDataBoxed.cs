#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System;

namespace Scellecs.Morpeh.WorldBrowser {
    [Serializable]
    internal struct ComponentDataBoxed {
        public IComponent data;
        public bool isMarker;
        public int typeId;
        public bool isNotSerialized;
    }
}
#endif