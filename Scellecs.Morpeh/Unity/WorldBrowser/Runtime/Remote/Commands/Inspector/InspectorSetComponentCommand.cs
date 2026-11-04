#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands
{
    [Serializable]
    internal struct InspectorSetComponentCommand : ICommand {
        public ComponentDataBoxed componentData;

        public byte CommandType() => CommandTypeId.Inspector;
        public byte CommandId() => InspectorCommand.SetComponent;
    }
}
#endif
