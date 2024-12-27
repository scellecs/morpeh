#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    [Serializable]
    internal struct BatchEndCommand : ICommand {
        public byte CommandType() => CommandTypeId.System;
        public byte CommandId() => SystemCommand.BatchEnd;
    }
}
#endif
