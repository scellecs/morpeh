#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct UpdateModelsResponse : ICommand {
        public byte CommandType() => CommandTypeId.Models;
        public byte CommandId() => ModelsCommand.UpdateResponse;
    }
}
#endif
