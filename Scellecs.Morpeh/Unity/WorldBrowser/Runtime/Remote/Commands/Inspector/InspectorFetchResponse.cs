#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct InspectorFetchResponse : ICommand {
        public SerializableInspectorModel inspector;

        public byte CommandType() => CommandTypeId.Inspector;
        public byte CommandId() => InspectorCommand.FetchResponse;
    }
}
#endif
