#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct HierarchyFetchResponse : ICommand {
        public SerializableHierarchyModel hierarchy;

        public byte CommandType() => CommandTypeId.Hierarchy;
        public byte CommandId() => HierarchyCommand.FetchResponse;
    }
}
#endif
