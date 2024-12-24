#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
