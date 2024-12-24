#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
