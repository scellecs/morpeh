#if MORPEH_REMOTE_BROWSER && (UNITY_EDITOR || UNITY_ASSERTIONS || UNITY_INCLUDE_INSTRUMENTATION)
using Scellecs.Morpeh.WorldBrowser.Remote;
using UnityEngine;

namespace Scellecs.Morpeh.Utils.WorldBrowser.Remote {
    public class ServerLogger : INetworkLogger {
        public void Log(string message) {
            //Debug.Log($"[Network] {message}");
        }
    }
}
#endif