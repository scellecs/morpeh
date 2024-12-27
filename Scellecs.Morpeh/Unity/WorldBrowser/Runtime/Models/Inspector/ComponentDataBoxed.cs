#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
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