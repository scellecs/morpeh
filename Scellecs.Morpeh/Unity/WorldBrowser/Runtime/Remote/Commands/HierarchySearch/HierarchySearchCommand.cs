#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal static class HierarchySearchCommand {
        internal const byte FetchRequest = 1;
        internal const byte SetComponentIncluded = 2;
        internal const byte SetSearchString = 3;
        internal const byte FetchResponse = 4;
    }
}
#endif
