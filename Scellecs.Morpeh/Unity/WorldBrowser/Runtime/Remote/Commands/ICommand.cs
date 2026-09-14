#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal interface ICommand {
        public byte CommandType();
        public byte CommandId();
    }
}
#endif
