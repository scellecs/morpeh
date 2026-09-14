#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using System.Collections.Generic;

namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal class CommandHandlerRegistry {
        private readonly Dictionary<byte, ICommandHandler> handlers = new Dictionary<byte, ICommandHandler>();

        internal void RegisterHandler(ICommandHandler handler) => handlers[handler.CommandType] = handler;
        internal bool TryGetHandler(byte commandType, out ICommandHandler handler) => handlers.TryGetValue(commandType, out handler);
    }
}
#endif
