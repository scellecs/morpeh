#if UNITY_EDITOR || DEVELOPMENT_BUILD
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