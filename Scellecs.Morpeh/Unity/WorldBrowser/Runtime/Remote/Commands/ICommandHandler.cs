#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal interface ICommandHandler {
        public byte CommandType { get; }
        public void Handle(Command command, NetworkTransport transport);
    }
}
#endif
