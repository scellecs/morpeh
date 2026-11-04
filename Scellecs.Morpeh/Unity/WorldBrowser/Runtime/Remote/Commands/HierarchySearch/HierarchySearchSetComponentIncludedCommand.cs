#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct HierarchySearchSetComponentIncludedCommand : ICommand {
        public int id;
        public bool included;
        public QueryParam param;

        public readonly byte CommandType() => Remote.CommandTypeId.HierarchySearch;
        public readonly byte CommandId() => (byte)HierarchySearchCommand.SetComponentIncluded;
    }
}
#endif
