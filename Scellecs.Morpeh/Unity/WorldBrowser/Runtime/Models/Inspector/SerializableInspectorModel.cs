#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;

namespace Scellecs.Morpeh.WorldBrowser {
    [Serializable]
    internal struct SerializableInspectorModel {
        public long version;
        public ComponentDataBoxed[] components;
        public int[] addComponentSuggestions;
        public Entity selectedEntity;
    }
}
#endif