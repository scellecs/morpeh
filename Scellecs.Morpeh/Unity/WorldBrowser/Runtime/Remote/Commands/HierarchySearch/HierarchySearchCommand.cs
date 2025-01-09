#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal static class HierarchySearchCommand {
        internal const byte FetchRequest = 1;
        internal const byte FetchResponse = 2;
        internal const byte SetComponentIncluded = 3;
        internal const byte SetSearchString = 4;
    }
}
#endif
