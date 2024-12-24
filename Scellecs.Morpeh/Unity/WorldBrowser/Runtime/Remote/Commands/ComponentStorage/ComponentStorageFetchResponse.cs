#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
