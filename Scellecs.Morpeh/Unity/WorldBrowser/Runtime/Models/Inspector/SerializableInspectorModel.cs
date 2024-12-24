#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

namespace Scellecs.Morpeh.WorldBrowser {
    [Serializable]
    internal struct SerializableInspectorModel {
        public long version;
        public ComponentDataBoxed[] components;
        public Entity selectedEntity;
    }
}
#endif