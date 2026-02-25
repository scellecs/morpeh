#if UNITY_EDITOR && MORPEH_REMOTE_BROWSER
using Scellecs.Morpeh.WorldBrowser.Remote;
using System;
using UnityEditor;
using UnityEngine.UIElements;
namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class RemoteToolbar : VisualElement {
        private const string TOOLBAR = "remote-toolbar";
        private const string BUTTON = "remote-toolbar-button";
        private const string IP_FIELD = "remote-toolbar-ip";

        private readonly Action<bool> OnStateChanged;

        internal RemoteToolbar(Action<bool> onRemoteStateChanged) {
            this.AddToClassList(TOOLBAR);
            this.OnStateChanged = onRemoteStateChanged;

            var connectButton = new Button(() => this.OnStateChanged?.Invoke(true));
            connectButton.text = "Connect to Remote";
            connectButton.AddToClassList(BUTTON);

            var ipField = new TextField("IP:") {
                value = EditorPrefs.GetString(RemoteWorldBrowserUtils.EDITOR_PREFS_IP_KEY, "127.0.0.1")
            };
            ipField.AddToClassList(IP_FIELD);
            ipField.RegisterValueChangedCallback(OnIpChanged);

            this.Add(connectButton);
            this.Add(ipField);
        }

        private void OnIpChanged(ChangeEvent<string> evt) {
            if (evt.newValue == evt.previousValue) {
                return;
            }

            EditorPrefs.SetString(RemoteWorldBrowserUtils.EDITOR_PREFS_IP_KEY, evt.newValue);
        }
    }
}
#endif