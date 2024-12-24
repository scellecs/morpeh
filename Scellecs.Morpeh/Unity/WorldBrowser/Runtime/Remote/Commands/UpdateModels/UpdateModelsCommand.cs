#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct UpdateModelsCommand : ICommand {
        public byte CommandType() => CommandTypeId.Models;
        public byte CommandId() => ModelsCommand.UpdateRequest;
    }
}
#endif
