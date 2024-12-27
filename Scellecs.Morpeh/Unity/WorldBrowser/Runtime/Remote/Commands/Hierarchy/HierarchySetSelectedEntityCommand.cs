#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct HierarchySetSelectedEntityCommand : ICommand {
        public Entity entity;

        public byte CommandType() => CommandTypeId.Hierarchy;
        public byte CommandId() => HierarchyCommand.SetSelectedEntity;
    }
}
#endif
