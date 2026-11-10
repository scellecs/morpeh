#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
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
