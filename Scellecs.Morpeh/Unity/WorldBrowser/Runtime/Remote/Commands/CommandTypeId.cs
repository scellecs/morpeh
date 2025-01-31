#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal static class CommandTypeId {
        internal const byte Models = 1;
        internal const byte ComponentStorage = 2;
        internal const byte HierarchySearch = 3;
        internal const byte Hierarchy = 4;
        internal const byte Inspector = 5;
        internal const byte System = 6;
    }
}
#endif
