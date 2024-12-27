#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
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
