#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
