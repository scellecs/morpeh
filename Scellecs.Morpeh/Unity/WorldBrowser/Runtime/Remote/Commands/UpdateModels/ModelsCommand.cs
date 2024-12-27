#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal static class ModelsCommand {
        internal const byte UpdateRequest = 1;
        internal const byte UpdateResponse = 2;
    }
}
#endif
