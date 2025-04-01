#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
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