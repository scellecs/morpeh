#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct HierarchySearchSetSearchStringCommand : ICommand {
        public string value;

        public readonly byte CommandType() => Remote.CommandTypeId.HierarchySearch;
        public readonly byte CommandId() => HierarchySearchCommand.SetSearchString;
    }
}
#endif
