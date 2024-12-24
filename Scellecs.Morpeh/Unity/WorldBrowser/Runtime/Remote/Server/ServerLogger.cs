#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Scellecs.Morpeh.WorldBrowser.Remote;
using UnityEngine;

namespace Scellecs.Morpeh.Utils.WorldBrowser.Remote {
    public class ServerLogger : INetworkLogger {
        public void Log(string message) {
            Debug.Log($"[Network] {message}");
        }
    }
}
#endif