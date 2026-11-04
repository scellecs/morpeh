#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct InspectorAddComponentCommand : ICommand {
        public int id;

        public byte CommandType() => CommandTypeId.Inspector;
        public byte CommandId() => InspectorCommand.AddComponent;
    }
}
#endif
