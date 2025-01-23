#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
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
