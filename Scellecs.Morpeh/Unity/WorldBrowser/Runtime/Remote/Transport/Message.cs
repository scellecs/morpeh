#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal unsafe struct Message {
        internal byte* data;
        internal int length;
    }
}
#endif
