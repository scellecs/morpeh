#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal unsafe struct Message {
        internal byte* data;
        internal int length;
    }
}
#endif
