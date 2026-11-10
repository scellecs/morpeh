#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal static class ComponentStorageCommand {
        internal const byte FetchRequest = 1;
        internal const byte FetchResponse = 2;
    }
}
#endif
