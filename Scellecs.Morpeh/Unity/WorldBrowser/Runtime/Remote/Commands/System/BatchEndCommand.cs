#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct BatchEndCommand : ICommand {
        public byte CommandType() => CommandTypeId.System;
        public byte CommandId() => SystemCommand.BatchEnd;
    }
}
#endif
