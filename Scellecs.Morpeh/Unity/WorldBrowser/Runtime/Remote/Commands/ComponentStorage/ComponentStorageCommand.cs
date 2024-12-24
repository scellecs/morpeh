#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal static class ComponentStorageCommand {
        internal const byte FetchRequest = 1;
        internal const byte FetchResponse = 2;
    }
}
#endif
