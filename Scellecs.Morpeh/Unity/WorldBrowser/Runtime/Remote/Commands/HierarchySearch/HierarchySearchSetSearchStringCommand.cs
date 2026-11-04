#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct HierarchySearchSetSearchStringCommand : ICommand {
        public string value;

        public readonly byte CommandType() => Remote.CommandTypeId.HierarchySearch;
        public readonly byte CommandId() => HierarchySearchCommand.SetSearchString;
    }
}
#endif
