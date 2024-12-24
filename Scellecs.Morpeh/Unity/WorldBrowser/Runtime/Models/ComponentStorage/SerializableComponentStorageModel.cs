#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

namespace Scellecs.Morpeh.WorldBrowser {
    [Serializable]
    internal struct SerializableComponentStorageModel {
        public long version;
        public string[] componentNames;
        public int[] keys;
        public int[] values;
    }
}
#endif
