#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal sealed class HierarchyCommand {
        internal const byte FetchRequest = 1;
        internal const byte SetSelectedEntity = 2;
        internal const byte SetSelectedWorldId = 3;
        internal const byte FetchResponse = 4;
    }
}
#endif
