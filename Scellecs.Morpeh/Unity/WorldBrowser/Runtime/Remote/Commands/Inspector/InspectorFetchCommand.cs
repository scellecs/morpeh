#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct InspectorFetchCommand : ICommand {
        public long version;

        public byte CommandType() => CommandTypeId.Inspector;
        public byte CommandId() => InspectorCommand.FetchRequest;
    }
}
#endif
