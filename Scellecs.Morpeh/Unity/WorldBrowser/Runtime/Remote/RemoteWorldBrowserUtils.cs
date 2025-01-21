#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER || DEVELOPMENT_BUILD && MORPEH_REMOTE_BROWSER
using System;

namespace Scellecs.Morpeh.WorldBrowser.Remote {
    internal static class RemoteWorldBrowserUtils {
        internal const int SERVER_PORT = 22005;
        internal const string EDITOR_PREFS_IP_KEY = "__WORLD_BROWSER_REMOTE_IP";

        internal static bool ParseIP(string input, out string ip, out int port) {
            ip = string.Empty;
            port = SERVER_PORT;

            if (string.IsNullOrEmpty(input)) {
                return false;
            }

            int separatorIndex = input.IndexOf(':');
            if (separatorIndex == -1) {
                ip = input;
                return true;
            }

            var ipSpan = input.AsSpan(0, separatorIndex);
            var portSpan = input.AsSpan()[(separatorIndex + 1)..];
            int.TryParse(portSpan, out port);
            ip = ipSpan.ToString();

            return !string.IsNullOrEmpty(ip);
        }
    }
}
#endif