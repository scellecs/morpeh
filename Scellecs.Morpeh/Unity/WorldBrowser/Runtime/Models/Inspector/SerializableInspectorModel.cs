#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
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