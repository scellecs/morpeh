#if UNITY_EDITOR || MORPEH_REMOTE_BROWSER && (UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
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
