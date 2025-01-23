#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal interface ICommandHandler {
        public byte CommandType { get; }
        public void Handle(Command command, NetworkTransport transport);
    }
}
#endif
