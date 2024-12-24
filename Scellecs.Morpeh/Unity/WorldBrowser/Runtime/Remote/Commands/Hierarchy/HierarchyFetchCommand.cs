#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct HierarchyFetchCommand : ICommand {
        public long version;

        public byte CommandType() => CommandTypeId.Hierarchy;
        public byte CommandId() => HierarchyCommand.FetchRequest;
    }
}
#endif
