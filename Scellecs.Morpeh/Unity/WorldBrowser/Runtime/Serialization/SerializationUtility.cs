#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using Unity.Serialization.Binary;

namespace Scellecs.Morpeh.WorldBrowser.Serialization {
    internal static class SerializationUtility {
        internal static List<IBinaryAdapter> binaryAdapters = new List<IBinaryAdapter>();

        static SerializationUtility() { 
            binaryAdapters.Add(new EntityAdapter());
            binaryAdapters.Add(new UnityObjectAdapter());
        }

        public static void AddAdapter(IBinaryAdapter adapter) {
            binaryAdapters.Add(adapter);
        }

        internal static T GetAdapter<T>() where T : IBinaryAdapter {
            for (int i = 0; i < binaryAdapters.Count; i++) {
                if (binaryAdapters[i] is T adapter) {
                    return adapter;
                }
            }

            return default;
        }
    }
}
#endif