#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct HierarchySearchFetchCommand : ICommand {
        public long version;

        public readonly byte CommandType() => CommandTypeId.HierarchySearch;
        public readonly byte CommandId() => HierarchySearchCommand.FetchRequest;
    }
}
#endif
