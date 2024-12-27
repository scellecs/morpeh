#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct UpdateModelsResponse : ICommand {
        public byte CommandType() => CommandTypeId.Models;
        public byte CommandId() => ModelsCommand.UpdateResponse;
    }
}
#endif
