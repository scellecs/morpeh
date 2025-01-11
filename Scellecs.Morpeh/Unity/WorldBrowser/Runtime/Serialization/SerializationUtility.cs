#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using Unity.Serialization.Binary;

namespace Scellecs.Morpeh.WorldBrowser.Serialization {
    internal unsafe static class SerializationUtility {
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

        internal static void WriteString(IBinarySerializationContext context, string str) {
            var byteCount = Encoding.UTF8.GetByteCount(str);

            if (byteCount <= 512) { 
                var bytes = stackalloc byte[byteCount];
                var actualLength = Encoding.UTF8.GetBytes(str, new Span<byte>(bytes, byteCount));

                context.Writer->Add(actualLength);
                context.Writer->Add(bytes, actualLength);
            }
            else {
                var array = ArrayPool<byte>.Shared.Rent(byteCount);
                var actualLength = Encoding.UTF8.GetBytes(str, array);
                context.Writer->Add(actualLength);
                fixed (byte* ptr = array) {
                    context.Writer->Add(ptr, actualLength);
                }

                ArrayPool<byte>.Shared.Return(array);
            }
        }

        internal static string ReadString(IBinaryDeserializationContext context) {
            var length = context.Reader->ReadNext<int>();
            var bytes = (byte*)context.Reader->ReadNext(length);
            return Encoding.UTF8.GetString(bytes, length);
        }
    }
}
#endif