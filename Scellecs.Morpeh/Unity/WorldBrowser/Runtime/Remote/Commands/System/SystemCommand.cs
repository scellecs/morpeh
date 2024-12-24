#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal static class SystemCommand {
        internal const byte BatchStart = 1;
        internal const byte BatchEnd = 2;
    }
}
#endif
