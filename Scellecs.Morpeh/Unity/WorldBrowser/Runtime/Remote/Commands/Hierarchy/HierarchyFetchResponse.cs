#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
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
