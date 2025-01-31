#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
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
