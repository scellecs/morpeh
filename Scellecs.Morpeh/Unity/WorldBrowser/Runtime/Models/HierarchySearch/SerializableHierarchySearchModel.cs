#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System;

namespace Scellecs.Morpeh.WorldBrowser {
    [Serializable]
    internal struct SerializableHierarchySearchModel {
        public long version;
        public string searchString;
        public int[] withSource;
        public int[] withoutSource;
        public int[] includedIds;
        public int[] excludedIds;
    }
}
#endif