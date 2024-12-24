#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal interface ICommandHandler {
        public byte CommandType { get; }
        public void Handle(Command command, NetworkTransport transport);
    }
}
#endif
