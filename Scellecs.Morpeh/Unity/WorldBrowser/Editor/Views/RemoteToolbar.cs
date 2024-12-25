#if UNITY_EDITOR
using System;
using UnityEngine.UIElements;
namespace Scellecs.Morpeh.WorldBrowser.Editor {
    internal sealed class RemoteToolbar : VisualElement {
        private const string TOOLBAR = "remote-toolbar";
        private const string BUTTON = "remote-toolbar-button";
        private readonly Action<bool> onStateChanged;

        internal RemoteToolbar(Action<bool> onRemoteStateChanged) {
            this.AddToClassList(TOOLBAR);
            this.onStateChanged = onRemoteStateChanged;

            var connectButton = new Button(() => this.onStateChanged?.Invoke(true));
            connectButton.text = "Connect to Remote";
            connectButton.AddToClassList(BUTTON);
            this.Add(connectButton);
        }
    }
}
#endif