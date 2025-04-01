#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser.Remote.Commands {
    internal static class SystemCommand {
        internal const byte BatchStart = 1;
        internal const byte BatchEnd = 2;
    }
}
#endif
