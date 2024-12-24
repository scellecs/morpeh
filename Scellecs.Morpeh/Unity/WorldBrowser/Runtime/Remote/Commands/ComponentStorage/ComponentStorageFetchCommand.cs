#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct ComponentStorageFetchCommand : ICommand {
        public long version;

        public byte CommandType() => CommandTypeId.ComponentStorage;
        public byte CommandId() => ComponentStorageCommand.FetchRequest;
    }
}
#endif
