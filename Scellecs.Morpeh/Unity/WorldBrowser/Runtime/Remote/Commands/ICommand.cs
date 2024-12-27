#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal interface ICommand {
        public byte CommandType();
        public byte CommandId();
    }
}
#endif
