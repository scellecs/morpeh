#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System;

namespace Scellecs.Morpeh.WorldBrowser {
    [Serializable]
    internal struct SerializableHierarchyModel {
        public long version;
        public int[] worldIds;
        public int[] selectedWorldIds;
        public Entity[] entities;
    }
}
#endif
