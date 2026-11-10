#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal static class ModelsCommand {
        internal const byte UpdateRequest = 1;
        internal const byte UpdateResponse = 2;
    }
}
#endif
