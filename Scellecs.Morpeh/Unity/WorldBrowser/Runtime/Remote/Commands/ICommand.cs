#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal interface ICommand {
        public byte CommandType();
        public byte CommandId();
    }
}
#endif
