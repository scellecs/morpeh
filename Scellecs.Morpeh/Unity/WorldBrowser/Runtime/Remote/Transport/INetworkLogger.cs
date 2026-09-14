#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
namespace Scellecs.Morpeh.WorldBrowser.Remote {
    public interface INetworkLogger { 
        public void Log(string message);
    }
}
#endif