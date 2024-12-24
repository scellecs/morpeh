#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Scellecs.Morpeh.WorldBrowser.Remote {
    public interface INetworkLogger { 
        public void Log(string message);
    }
}
#endif