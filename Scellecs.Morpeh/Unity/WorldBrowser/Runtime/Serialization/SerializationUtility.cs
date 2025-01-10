#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System.Collections.Generic;
using System.Text;
using Unity.Serialization.Binary;

namespace Scellecs.Morpeh.WorldBrowser.Serialization {
    internal unsafe static class SerializationUtility {
        private const int MAX_STRING_LENGTH = 256;

        internal static List<IBinaryAdapter> binaryAdapters = new List<IBinaryAdapter>();

        static SerializationUtility() {
            binaryAdapters.Add(new BoxedComponentAdapter());
            binaryAdapters.Add(new EntityAdapter());
        }

        public static void AddAdapter(IBinaryAdapter adapter) {
            for (int i = 0; i < binaryAdapters.Count; i++) {
                var binaryAdapter = binaryAdapters[i];
                if (binaryAdapter.GetType() == adapter.GetType()) {
                    return;
                }
            }

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

        internal static void WriteString(IBinarySerializationContext context, string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            context.Writer->Add(bytes.Length);
            fixed (byte* ptr = bytes)
            {
                context.Writer->Add(ptr, bytes.Length);
            }
        }

        internal static string ReadString(IBinaryDeserializationContext context)
        {
            var length = context.Reader->ReadNext<int>();
            if (length <= 0 || length > MAX_STRING_LENGTH)
            {
                return string.Empty;
            }
            byte* bytes = (byte*)context.Reader->ReadNext(length);
            return Encoding.UTF8.GetString(bytes, length);
        }
    }
}
#endif