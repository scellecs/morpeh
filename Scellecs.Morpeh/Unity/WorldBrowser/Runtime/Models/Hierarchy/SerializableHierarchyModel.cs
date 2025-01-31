#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
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
