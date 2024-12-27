#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
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
