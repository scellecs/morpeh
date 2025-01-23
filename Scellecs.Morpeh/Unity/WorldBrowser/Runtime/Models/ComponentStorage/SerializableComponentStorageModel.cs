#if UNITY_EDITOR || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
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
