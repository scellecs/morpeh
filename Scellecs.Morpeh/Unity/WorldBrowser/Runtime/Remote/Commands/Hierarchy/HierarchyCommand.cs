#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal sealed class HierarchyCommand {
        internal const byte FetchRequest = 1;
        internal const byte FetchResponse = 2;
        internal const byte SetSelectedEntity = 3;
        internal const byte SetSelectedWorldId = 4;
    }
}
#endif
