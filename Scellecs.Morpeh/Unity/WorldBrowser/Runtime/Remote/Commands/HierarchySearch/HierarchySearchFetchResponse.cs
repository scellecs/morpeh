#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Scellecs.Morpeh.WorldBrowser.Remote.Commands;
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote {
    [Serializable]
    internal struct HierarchySearchFetchResponse : ICommand {
        public SerializableHierarchySearchModel hierarchySearch;

        public byte CommandType() => CommandTypeId.HierarchySearch;
        public byte CommandId() => HierarchySearchCommand.FetchResponse;
    }
}
#endif
