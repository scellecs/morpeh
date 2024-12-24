#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct HierarchySetSelectedWorldIdCommand : ICommand {
        public int worldId;
        public bool state;

        public byte CommandType() => CommandTypeId.Hierarchy;
        public byte CommandId() => HierarchyCommand.SetSelectedWorldId;
    }
}
#endif
