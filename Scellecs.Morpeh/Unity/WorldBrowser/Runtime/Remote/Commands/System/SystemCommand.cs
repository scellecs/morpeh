#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal static class SystemCommand {
        internal const byte BatchStart = 1;
        internal const byte BatchEnd = 2;
    }
}
#endif
