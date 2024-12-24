#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal static class ModelsCommand {
        internal const byte UpdateRequest = 1;
        internal const byte UpdateResponse = 2;
    }
}
#endif
