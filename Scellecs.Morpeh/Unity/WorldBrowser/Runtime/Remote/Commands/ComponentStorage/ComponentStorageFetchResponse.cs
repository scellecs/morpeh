#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct ComponentStorageFetchResponse : ICommand {
        public SerializableComponentStorageModel storage;

        public byte CommandType() => CommandTypeId.ComponentStorage;
        public byte CommandId() => ComponentStorageCommand.FetchResponse;
    }
}
#endif
