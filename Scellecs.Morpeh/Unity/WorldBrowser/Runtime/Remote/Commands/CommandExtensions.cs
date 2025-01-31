#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using Scellecs.Morpeh.WorldBrowser.Serialization;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Serialization.Binary;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal unsafe static class CommandExtensions {
        internal static T Deserialize<T>(this Command command) where T : struct, ICommand {
            var parameters = new BinarySerializationParameters {
                UserDefinedAdapters = SerializationUtility.binaryAdapters,
            };
            var streamReader = new UnsafeAppendBuffer.Reader(command.Data, command.Length);
            return BinarySerialization.FromBinary<T>(&streamReader, parameters);
        }

        internal static byte* Serialize<T>(this T command, NetworkAllocator allocator, out int length) where T : struct, ICommand {
            using var stream = new UnsafeAppendBuffer(16, 8, Allocator.Temp);
            var parameters = new BinarySerializationParameters {
                UserDefinedAdapters = SerializationUtility.binaryAdapters,
            };
            BinarySerialization.ToBinary(&stream, command, parameters);
            length = stream.Length + 2;
            var result = allocator.Alloc(length);
            result[0] = command.CommandType();
            result[1] = command.CommandId();
            if (stream.Length > 0) {
                UnsafeUtility.MemCpy(result + 2, stream.Ptr, stream.Length);
            }
            return result;
        }
    }
}
#endif
